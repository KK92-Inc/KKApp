import {
  Body,
  Button,
  Container,
  Head,
  Heading,
  Hr,
  Html,
  Img,
  Link,
  Preview,
  Section,
  Tailwind,
  Text,
} from '@react-email/components';

interface WelcomeUserEmailProps {
  username?: string;
  userEmail?: string;
  platformName?: string;
  dashboardLink?: string;
  supportEmail?: string;
}

const baseUrl = process.env.VERCEL_URL
  ? `https://${process.env.VERCEL_URL}`
  : '';

export const WelcomeUserEmail = ({
  username,
  userEmail,
  platformName,
  dashboardLink,
  supportEmail,
}: WelcomeUserEmailProps) => {
  const previewText = `Welcome to ${platformName}!`;

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
                alt={platformName}
                className="mx-auto my-0"
              />
            </Section>
            <Heading className="mx-0 my-[30px] p-0 text-center font-normal text-[24px] text-black">
              Welcome to <strong>{platformName}</strong>!
            </Heading>
            <Text className="text-[14px] text-black leading-[24px]">
              Hello {username},
            </Text>
            <Text className="text-[14px] text-black leading-[24px]">
              We're excited to have you on board! Your account has been
              successfully created with the email{' '}
              <Link
                href={`mailto:${userEmail}`}
                className="text-blue-600 no-underline"
              >
                {userEmail}
              </Link>
              .
            </Text>
            <Text className="text-[14px] text-black leading-[24px]">
              Here's what you can do next:
            </Text>
            <ul className="text-[14px] text-black leading-[24px]">
              <li>Complete your profile</li>
              <li>Explore available projects</li>
              <li>Connect with other users</li>
              <li>Start your first evaluation</li>
            </ul>
            <Section className="mt-[32px] mb-[32px] text-center">
              <Button
                className="rounded bg-[#000000] px-5 py-3 text-center font-semibold text-[12px] text-white no-underline"
                href={dashboardLink}
              >
                Go to Dashboard
              </Button>
            </Section>
            <Text className="text-[14px] text-black leading-[24px]">
              or copy and paste this URL into your browser:{' '}
              <Link href={dashboardLink} className="text-blue-600 no-underline">
                {dashboardLink}
              </Link>
            </Text>
            <Hr className="mx-0 my-[26px] w-full border border-[#eaeaea] border-solid" />
            <Text className="text-[#666666] text-[12px] leading-[24px]">
              If you have any questions, feel free to reach out to our support
              team at{' '}
              <Link
                href={`mailto:${supportEmail}`}
                className="text-blue-600 no-underline"
              >
                {supportEmail}
              </Link>
              . We're here to help!
            </Text>
          </Container>
        </Body>
      </Tailwind>
    </Html>
  );
};

WelcomeUserEmail.PreviewProps = {
  username: 'alanturing',
  userEmail: 'alan.turing@example.com',
  platformName: 'NxtApp',
  dashboardLink: 'https://nxtapp.com/dashboard',
  supportEmail: 'support@nxtapp.com',
} as WelcomeUserEmailProps;

export default WelcomeUserEmail;
