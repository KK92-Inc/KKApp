# Kit Proxy

Since we use SvelteKit as both a Middleman and Proxy sometimes in some places
we may require handling specific api calls.

For example using Server Sent Events from the Backend we can proxy the authentication
part and have hooks.server.ts deal with it.
