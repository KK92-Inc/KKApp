import { env } from "$env/dynamic/private";

export const redis = new Bun.RedisClient(env.CACHE_URI);
