import {
  Body,
  Button,
  Column,
  Container,
  Head,
  Heading,
  Hr,
  Html,
  Img,
  Link,
  Preview,
  Row,
  Section,
  Tailwind,
  Text,
} from '@react-email/components';

type EventType =
  | 'deadline'
  | 'workshop'
  | 'webinar'
  | 'meeting'
  | 'announcement'
  | 'maintenance'
  | 'other';

interface EventNotificationEmailProps {
  username?: string;
  eventTitle?: string;
  eventType?: EventType;
  eventDescription?: string;
  eventDate?: string;
  eventTime?: string;
  eventTimezone?: string;
  eventLocation?: string;
  eventLink?: string;
  calendarLink?: string;
  organizerName?: string;
  organizerEmail?: string;
  additionalInfo?: string;
}

const baseUrl = process.env.VERCEL_URL
  ? `https://${process.env.VERCEL_URL}`
  : '';

const getEventIcon = (type: EventType): string => {
  switch (type) {
    case 'deadline':
      return `${baseUrl}/static/deadline-icon.png`;
    case 'workshop':
      return `${baseUrl}/static/workshop-icon.png`;
    case 'webinar':
      return `${baseUrl}/static/webinar-icon.png`;
    case 'meeting':
      return `${baseUrl}/static/meeting-icon.png`;
    case 'announcement':
      return `${baseUrl}/static/announcement-icon.png`;
    case 'maintenance':
      return `${baseUrl}/static/maintenance-icon.png`;
    default:
      return `${baseUrl}/static/event-icon.png`;
  }
};

const getEventTypeLabel = (type: EventType): string => {
  switch (type) {
    case 'deadline':
      return '⏰ Deadline';
    case 'workshop':
      return '🛠️ Workshop';
    case 'webinar':
      return '🎥 Webinar';
    case 'meeting':
      return '👥 Meeting';
    case 'announcement':
      return '📢 Announcement';
    case 'maintenance':
      return '🔧 Maintenance';
    default:
      return '📅 Event';
  }
};

const getEventColor = (type: EventType): string => {
  switch (type) {
    case 'deadline':
      return '#dc2626';
    case 'workshop':
      return '#7c3aed';
    case 'webinar':
      return '#2563eb';
    case 'meeting':
      return '#059669';
    case 'announcement':
      return '#d97706';
    case 'maintenance':
      return '#6b7280';
    default:
      return '#000000';
  }
};

export const EventNotificationEmail = ({
  username,
  eventTitle,
  eventType,
  eventDescription,
  eventDate,
  eventTime,
  eventTimezone,
  eventLocation,
  eventLink,
  calendarLink,
  organizerName,
  organizerEmail,
  additionalInfo,
}: EventNotificationEmailProps) => {
  const previewText = `${getEventTypeLabel(eventType || 'other')}: ${eventTitle}`;
  const eventColor = getEventColor(eventType || 'other');

  return (
    <Html>
      <Head />
      <Tailwind>
        <Body className="mx-auto my-auto bg-white px-2 font-sans">
          <Preview>{previewText}</Preview>
          <Container className="mx-auto my-[40px] max-w-[465px] rounded border border-[#eaeaea] border-solid p-[20px]">
            <Section className="mt-[32px]">
              <Img
                src={`${baseUrl}/static/logo.png`}
                width="40"
                height="37"
                alt="Logo"
                className="mx-auto my-0"
              />
            </Section>
            <Section className="mt-[16px] text-center">
              <Text
                className="m-0 font-semibold text-[12px]"
                style={{ color: eventColor }}
              >
                {getEventTypeLabel(eventType || 'other')}
              </Text>
            </Section>
            <Heading className="mx-0 my-[20px] p-0 text-center font-normal text-[24px] text-black">
              {eventTitle}
            </Heading>
            <Text className="text-[14px] text-black leading-[24px]">
              Hello {username},
            </Text>
            <Text className="text-[14px] text-black leading-[24px]">
              {eventDescription}
            </Text>
            <Section className="mt-[16px] mb-[16px] text-center">
              <Img
                src={getEventIcon(eventType || 'other')}
                width="64"
                height="64"
                alt="Event"
                className="mx-auto my-0"
              />
            </Section>
            <Section className="mt-[16px] rounded bg-[#f6f6f6] p-[16px]">
              <Text className="m-0 text-[14px] text-[#666666] leading-[24px]">
                <strong>Event Details:</strong>
              </Text>
              {eventDate && (
                <Row className="mt-[8px]">
                  <Column style={{ width: '24px' }}>
                    <Text className="m-0 text-[14px]">📅</Text>
                  </Column>
                  <Column>
                    <Text className="m-0 text-[14px] text-black leading-[24px]">
                      {eventDate}
                    </Text>
                  </Column>
                </Row>
              )}
              {eventTime && (
                <Row className="mt-[4px]">
                  <Column style={{ width: '24px' }}>
                    <Text className="m-0 text-[14px]">🕐</Text>
                  </Column>
                  <Column>
                    <Text className="m-0 text-[14px] text-black leading-[24px]">
                      {eventTime}
                      {eventTimezone ? ` (${eventTimezone})` : ''}
                    </Text>
                  </Column>
                </Row>
              )}
              {eventLocation && (
                <Row className="mt-[4px]">
                  <Column style={{ width: '24px' }}>
                    <Text className="m-0 text-[14px]">📍</Text>
                  </Column>
                  <Column>
                    <Text className="m-0 text-[14px] text-black leading-[24px]">
                      {eventLocation}
                    </Text>
                  </Column>
                </Row>
              )}
              {organizerName && (
                <Row className="mt-[4px]">
                  <Column style={{ width: '24px' }}>
                    <Text className="m-0 text-[14px]">👤</Text>
                  </Column>
                  <Column>
                    <Text className="m-0 text-[14px] text-black leading-[24px]">
                      Organized by{' '}
                      {organizerEmail ? (
                        <Link
                          href={`mailto:${organizerEmail}`}
                          className="text-blue-600 no-underline"
                        >
                          {organizerName}
                        </Link>
                      ) : (
                        organizerName
                      )}
                    </Text>
                  </Column>
                </Row>
              )}
            </Section>
            {additionalInfo && (
              <Section className="mt-[16px]">
                <Text className="text-[14px] text-black leading-[24px]">
                  <strong>Additional Information:</strong>
                </Text>
                <Text className="text-[14px] text-[#666666] leading-[24px]">
                  {additionalInfo}
                </Text>
              </Section>
            )}
            <Section className="mt-[32px] mb-[32px] text-center">
              <Row>
                <Column align="center">
                  {eventLink && (
                    <Button
                      className="rounded bg-[#000000] px-5 py-3 text-center font-semibold text-[12px] text-white no-underline"
                      href={eventLink}
                    >
                      View Event
                    </Button>
                  )}
                </Column>
                {calendarLink && (
                  <Column align="center">
                    <Button
                      className="rounded border border-[#000000] border-solid bg-white px-5 py-3 text-center font-semibold text-[12px] text-black no-underline"
                      href={calendarLink}
                    >
                      Add to Calendar
                    </Button>
                  </Column>
                )}
              </Row>
            </Section>
            {eventLink && (
              <Text className="text-[14px] text-black leading-[24px]">
                or copy and paste this URL into your browser:{' '}
                <Link href={eventLink} className="text-blue-600 no-underline">
                  {eventLink}
                </Link>
              </Text>
            )}
            <Hr className="mx-0 my-[26px] w-full border border-[#eaeaea] border-solid" />
            <Text className="text-[#666666] text-[12px] leading-[24px]">
              You're receiving this notification because you're registered on
              our platform. If you believe this was sent in error, please
              contact our support team.
            </Text>
          </Container>
        </Body>
      </Tailwind>
    </Html>
  );
};

EventNotificationEmail.PreviewProps = {
  username: 'alanturing',
  eventTitle: 'Project Submission Deadline',
  eventType: 'deadline',
  eventDescription:
    'This is a friendly reminder that your project submission deadline is approaching. Please ensure all your work is submitted before the deadline.',
  eventDate: 'February 15, 2026',
  eventTime: '11:59 PM',
  eventTimezone: 'UTC',
  eventLocation: 'Online Submission Portal',
  eventLink: 'https://nxtapp.com/events/deadline-123',
  calendarLink: 'https://nxtapp.com/calendar/add/deadline-123',
  organizerName: 'NxtApp Team',
  organizerEmail: 'team@nxtapp.com',
  additionalInfo:
    'Late submissions will not be accepted. Please reach out to support if you need an extension.',
} as EventNotificationEmailProps;

export default EventNotificationEmail;
