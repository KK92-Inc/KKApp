#!/usr/bin/env python3
"""
Tiny local agent: Ollama (tool-calling model) <-> KKApp MCP server (/mcp).

  python kk_agent.py                          # interactive chat
  python kk_agent.py "Could you list me the users?"   # one-shot

Config via environment variables (see OLLAMA-GUIDE.md):
  KK_MCP_URL        default http://localhost:5145/mcp
  KK_TOKEN          a ready bearer token (skips the Keycloak fetch), OR set the 5 below:
  KK_KC_URL         e.g. http://keycloak-w2inc.dev.localhost:8080
  KK_CLIENT_SECRET  the kc-student-intra-secret value
  KK_USERNAME / KK_PASSWORD   your Keycloak staff test user
  KK_CLIENT_ID      default intra
  OLLAMA_URL        default http://localhost:11434
  OLLAMA_MODEL      default qwen3:8b
  KK_THINK          set to true/false to send Ollama's `think` flag (thinking models only)
  KK_AUTO_APPROVE   set to 1 to skip the y/n prompt before non-read-only tools (not recommended)
"""
import asyncio, json, os, sys
import httpx
from mcp import ClientSession

# The MCP Python SDK changed its HTTP client helper between 1.x and 2.x; support both.
try:
    from mcp.client.streamable_http import streamablehttp_client  # 1.x

    def open_mcp(url, headers):
        return streamablehttp_client(url, headers=headers)
except ImportError:  # 2.x
    from mcp.client.streamable_http import streamable_http_client, create_mcp_http_client

    def open_mcp(url, headers):
        return streamable_http_client(url, http_client=create_mcp_http_client(headers=headers))

MCP_URL = os.getenv("KK_MCP_URL", "http://localhost:5145/mcp")
OLLAMA = os.getenv("OLLAMA_URL", "http://localhost:11434").rstrip("/")
MODEL = os.getenv("OLLAMA_MODEL", "qwen3:8b")
MAX_STEPS = 8

SYSTEM = """You are an administrative assistant for the KKApp learning platform.
You can only act through the provided tools. Rules:
- For any question about users or freezes, CALL A TOOL. Never invent data.
- Never guess a user id: find it with list_users first.
- Before freeze_user, unfreeze_user or anonymize_user, state who you are about to affect.
- If a tool returns an error, tell the user the error verbatim; do not retry blindly.
- Be concise."""


def get_token() -> str:
    if os.getenv("KK_TOKEN"):
        return os.environ["KK_TOKEN"]
    need = ["KK_KC_URL", "KK_CLIENT_SECRET", "KK_USERNAME", "KK_PASSWORD"]
    missing = [n for n in need if not os.getenv(n)]
    if missing:
        sys.exit(f"Set KK_TOKEN, or all of: {', '.join(need)} (missing: {', '.join(missing)})")
    r = httpx.post(
        f"{os.environ['KK_KC_URL'].rstrip('/')}/realms/student/protocol/openid-connect/token",
        data={
            "grant_type": "password",
            "client_id": os.getenv("KK_CLIENT_ID", "intra"),
            "client_secret": os.environ["KK_CLIENT_SECRET"],
            "username": os.environ["KK_USERNAME"],
            "password": os.environ["KK_PASSWORD"],
            "scope": "openid user",
        },
        timeout=15,
    )
    if r.status_code != 200:
        sys.exit(f"Keycloak token request failed ({r.status_code}): {r.text[:300]}")
    return r.json()["access_token"]


def simplify(schema: dict) -> dict:
    """Small models trip over JSON-schema extras (type arrays, defaults, formats). Flatten them."""
    if not isinstance(schema, dict):
        return schema
    out = {}
    for k, v in schema.items():
        if k in ("default", "format"):
            continue
        if k == "type" and isinstance(v, list):
            v = next((t for t in v if t != "null"), "string")
        out[k] = simplify(v) if isinstance(v, dict) else v
    if "properties" in out:
        out["properties"] = {n: simplify(p) for n, p in out["properties"].items()}
    return out


def to_ollama_tools(mcp_tools):
    return [
        {
            "type": "function",
            "function": {
                "name": t.name,
                "description": t.description or "",
                "parameters": simplify(attr(t, "inputSchema", "input_schema") or {"type": "object", "properties": {}}),
            },
        }
        for t in mcp_tools
    ]


def attr(obj, *names):
    """First attribute that exists (the 1.x SDK uses camelCase, 2.x snake_case)."""
    for n in names:
        if getattr(obj, n, None) is not None:
            return getattr(obj, n)
    return None


def is_read_only(tool) -> bool:
    ann = getattr(tool, "annotations", None)
    return bool(ann and attr(ann, "readOnlyHint", "read_only_hint"))


async def chat(client: httpx.AsyncClient, messages, tools):
    # Explicitly set "think": False by default
    body = {
        "model": MODEL, 
        "messages": messages, 
        "tools": tools, 
        "stream": False, 
        "think": False
    }
    
    # Allow overriding via KK_THINK environment variable if desired
    if os.getenv("KK_THINK"):
        body["think"] = os.environ["KK_THINK"].lower() == "true"
        
    r = await client.post(f"{OLLAMA}/api/chat", json=body, timeout=600)
    if r.status_code != 200:
        sys.exit(f"Ollama error {r.status_code}: {r.text[:400]}")
    return r.json()["message"]


async def run_turn(session, client, tools_by_name, ollama_tools, messages):
    for _ in range(MAX_STEPS):
        msg = await chat(client, messages, ollama_tools)
        messages.append(msg)
        calls = msg.get("tool_calls") or []
        if not calls:
            return msg.get("content", "").strip()

        for call in calls:
            fn = call["function"]
            name, args = fn["name"], fn.get("arguments") or {}
            if isinstance(args, str):
                args = json.loads(args or "{}")
            print(f"\n  -> tool call: {name}({json.dumps(args)})")

            tool = tools_by_name.get(name)
            if tool is None:
                result = f"Error: unknown tool '{name}'."
            elif not is_read_only(tool) and os.getenv("KK_AUTO_APPROVE") != "1" \
                    and input("     This tool can change data. Allow? [y/N] ").strip().lower() != "y":
                result = "The human declined this action. Do not retry; ask what they want instead."
            else:
                res = await session.call_tool(name, args)
                result = "\n".join(c.text for c in res.content if getattr(c, "text", None)) or "(empty result)"
                if attr(res, "isError", "is_error"):
                    result = f"Tool error: {result}"
            print(f"  <- {result[:300]}{'...' if len(result) > 300 else ''}")
            messages.append({"role": "tool", "tool_name": name, "content": result})
    return "(stopped: too many tool steps)"


async def main():
    one_shot = " ".join(sys.argv[1:]).strip() or None
    headers = {"Authorization": f"Bearer {get_token()}"}

    async with open_mcp(MCP_URL, headers) as streams:
        read, write = streams[0], streams[1]
        async with ClientSession(read, write) as session:
            await session.initialize()
            tools = (await session.list_tools()).tools
            tools_by_name = {t.name: t for t in tools}
            ollama_tools = to_ollama_tools(tools)
            print(f"Connected: {len(tools)} tools from {MCP_URL}; model = {MODEL}")

            messages = [{"role": "system", "content": SYSTEM}]
            async with httpx.AsyncClient() as client:
                while True:
                    q = one_shot or input("\nyou> ").strip()
                    if not q:
                        continue
                    if q.lower() in ("exit", "quit"):
                        break
                    messages.append({"role": "user", "content": q})
                    print("\nassistant>", await run_turn(session, client, tools_by_name, ollama_tools, messages))
                    if one_shot:
                        break


if __name__ == "__main__":
    asyncio.run(main())