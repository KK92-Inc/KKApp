import type { components } from '$lib/api/api';

// ============================================================================
// Proper discriminated union for notification data payloads.
// The generated API types are stale and only know about MessageDO; define the
// full union here until the openapi types are regenerated.
// ============================================================================

export type MessageData = {
	type: 'Message';
	text: string;
	html: string;
};

export type ProjectInviteData = {
	type: 'ProjectInvite';
	userProjectId: string;
	inviterUserId: string;
};

export type FeedNotificationData = MessageData | ProjectInviteData;

/** NotificationDO with a properly-typed data field */
export type FeedNotification = Omit<components['schemas']['NotificationDO'], 'data'> & {
	data?: FeedNotificationData;
};

// ============================================================================
// NotificationMeta flag values (mirrors the C# [Flags] enum)
// ============================================================================

export const Meta = {
	AcceptOrDecline: 1 << 0, //   1
	Project: 1 << 5, //  32
	Goal: 1 << 6, //  64
	Cursus: 1 << 7, // 128
	Review: 1 << 8, // 256
	User: 1 << 9, // 512
	Feed: 1 << 10 // 1024
} as const;

export const hasFlag = (descriptor: number | undefined, flag: number): boolean =>
	((descriptor ?? 0) & flag) !== 0;

export const relativeTime = (dateString: string): string => {
	const diffDays = Math.round((new Date(dateString).getTime() - Date.now()) / 86_400_000);
	return new Intl.RelativeTimeFormat('en', { numeric: 'auto' }).format(diffDays, 'days');
};
