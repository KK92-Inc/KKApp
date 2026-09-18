// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { env } from "$env/dynamic/public";

// ============================================================================

export const ALLOWED_IMAGE_TYPES = ['image/png', 'image/jpeg', 'image/gif', 'image/webp'] as const;
export type AllowedImageType = (typeof ALLOWED_IMAGE_TYPES)[number];

export const MAX_IMAGE_MB = 5;
export const MAX_IMAGE_BYTES = MAX_IMAGE_MB * 1024 * 1024;
export const MAX_IMAGE_PIXELS = 4096 * 4096; // decompression-bomb guard

// ============================================================================

function parseDataUrl(dataUrl: string): Buffer {
  const match = /^data:([\w/+.-]+);base64,(.+)$/.exec(dataUrl);
  if (!match) throw new Error('Invalid data URL');

  const [, mime, base64] = match;
  if (!ALLOWED_IMAGE_TYPES.includes(mime as (typeof ALLOWED_IMAGE_TYPES)[number])) {
    throw new Error(`Unsupported image type: ${mime}`);
  }

  const buffer = Buffer.from(base64, 'base64');
  if (buffer.byteLength === 0) throw new Error('Empty image payload');
  if (buffer.byteLength > MAX_IMAGE_BYTES) throw new Error('Image too large');
  return buffer;
}

export interface ImageStoreOptions {
  bucket: string;
  width?: number;   // resize target
  height?: number;  // defaults to `width` (square)
  quality?: number; // webp quality
}

export function useS3Storage({
  bucket,
  width = 512,
  height = width,
  quality = 82
}: ImageStoreOptions) {
  const keyFor = (id: string) => `${id}/thumbnail.webp`;

  return {
    async write(id: string, dataUrl: string): Promise<string> {
      const buffer = parseDataUrl(dataUrl);

      const pipeline = new Bun.Image(buffer, { maxPixels: MAX_IMAGE_PIXELS })
        .resize(width, height, { fit: 'inside', withoutEnlargement: true })
        .webp({ quality });

      const file = Bun.s3.file(keyFor(id), { bucket, endpoint: env.PUBLIC_S3_ENDPOINT });
      await pipeline.write(file);

      // public bucket/CDN? build the URL directly instead:
      return `${env.PUBLIC_S3_ENDPOINT}/${bucket}/${keyFor(id)}`;
      // return file.presign({ acl: 'public-read', expiresIn: 60 * 60 * 24 * 365 });
    },

    async delete(id: string): Promise<void> {
      await Bun.s3.file(keyFor(id), { bucket }).delete().catch(() => {});
    }
  };
}
