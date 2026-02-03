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

interface ProjectInviteEmailProps {
  username?: string;
  userImage?: string;
  invitedByUsername?: string;
  invitedByEmail?: string;
  invitedByImage?: string;
  projectName?: string;
  projectDescription?: string;
  projectImage?: string;
  role?: string;
  inviteLink?: string;
  inviteFromIp?: string;
  inviteFromLocation?: string;
}

const baseUrl = process.env.VERCEL_URL
  ? `https://${process.env.VERCEL_URL}`
  : '';

export const ProjectInviteEmail = ({
  username,
  userImage,
  invitedByUsername,
  invitedByEmail,
  invitedByImage,
  projectName,
  projectDescription,
  projectImage,
  role,
  inviteLink,
  inviteFromIp,
  inviteFromLocation,
}: ProjectInviteEmailProps) => {
  const previewText = `${invitedByUsername} invited you to collaborate on ${projectName}`;

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
            <Heading className="mx-0 my-[30px] p-0 text-center font-normal text-[24px] text-black">
              Join <strong>{projectName}</strong>
            </Heading>
            <Text className="text-[14px] text-black leading-[24px]">
              Hello {username},
            </Text>
            <Text className="text-[14px] text-black leading-[24px]">
              <strong>{invitedByUsername}</strong> (
              <Link
                href={`mailto:${invitedByEmail}`}
                className="text-blue-600 no-underline"
              >
                {invitedByEmail}
              </Link>
              ) has invited you to collaborate on the project{' '}
              <strong>{projectName}</strong> as a <strong>{role}</strong>.
            </Text>
            <Section>
              <Row>
                <Column align="right">
                  <Img
                    className="rounded-full"
                    src={invitedByImage}
                    width="64"
                    height="64"
                  />
                </Column>
                <Column align="center">
                  <Img
                    src={`${baseUrl}/static/arrow.png`}
                    width="12"
                    height="9"
                    alt="invited you to"
                  />
                </Column>
                <Column align="left">
                  <Img
                    className="rounded-full"
                    src={projectImage}
                    width="64"
                    height="64"
                  />
                </Column>
              </Row>
            </Section>
            {projectDescription && (
              <Section className="mt-[16px] rounded bg-[#f6f6f6] p-[16px]">
                <Text className="m-0 text-[14px] text-[#666666] leading-[24px]">
                  <strong>About this project:</strong>
                </Text>
                <Text className="m-0 text-[14px] text-black leading-[24px]">
                  {projectDescription}
                </Text>
              </Section>
            )}
            <Section className="mt-[32px] mb-[32px] text-center">
              <Button
                className="rounded bg-[#000000] px-5 py-3 text-center font-semibold text-[12px] text-white no-underline"
                href={inviteLink}
              >
                Accept Invitation
              </Button>
            </Section>
            <Text className="text-[14px] text-black leading-[24px]">
              or copy and paste this URL into your browser:{' '}
              <Link href={inviteLink} className="text-blue-600 no-underline">
                {inviteLink}
              </Link>
            </Text>
            <Hr className="mx-0 my-[26px] w-full border border-[#eaeaea] border-solid" />
            <Text className="text-[#666666] text-[12px] leading-[24px]">
              This invitation was intended for{' '}
              <span className="text-black">{username}</span>. This invite was
              sent from <span className="text-black">{inviteFromIp}</span>{' '}
              located in{' '}
              <span className="text-black">{inviteFromLocation}</span>. If you
              were not expecting this invitation, you can ignore this email. If
              you are concerned about your account's safety, please reply to
              this email to get in touch with us.
            </Text>
          </Container>
        </Body>
      </Tailwind>
    </Html>
  );
};

ProjectInviteEmail.PreviewProps = {
  username: 'alanturing',
  userImage: `${baseUrl}/static/user.png`,
  invitedByUsername: 'Ada Lovelace',
  invitedByEmail: 'ada.lovelace@example.com',
  invitedByImage: `${baseUrl}/static/inviter.png`,
  projectName: 'Analytical Engine',
  projectDescription:
    'A revolutionary computing project that aims to perform complex calculations and store data.',
  projectImage: `${baseUrl}/static/project.png`,
  role: 'Collaborator',
  inviteLink: 'https://nxtapp.com/projects/invite/abc123',
  inviteFromIp: '192.168.1.1',
  inviteFromLocation: 'London, United Kingdom',
} as ProjectInviteEmailProps;

export default ProjectInviteEmail;
