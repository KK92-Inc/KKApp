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

type EvaluatorType = 'bot' | 'international' | 'peer';

interface IncomingEvaluationEmailProps {
  username?: string;
  projectName?: string;
  projectLink?: string;
  evaluatorType?: EvaluatorType;
  evaluatorName?: string;
  evaluatorCountry?: string;
  expectedCompletionDate?: string;
  evaluationCriteria?: string[];
}

const baseUrl = process.env.VERCEL_URL
  ? `https://${process.env.VERCEL_URL}`
  : '';

const getEvaluatorDescription = (
  type: EvaluatorType,
  name?: string,
  country?: string
): string => {
  switch (type) {
    case 'bot':
      return 'an automated evaluation system';
    case 'international':
      return `an international reviewer${country ? ` from ${country}` : ''}`;
    case 'peer':
      return `your peer${name ? ` ${name}` : ''}`;
    default:
      return 'a reviewer';
  }
};

const getEvaluatorIcon = (type: EvaluatorType): string => {
  switch (type) {
    case 'bot':
      return `${baseUrl}/static/bot-icon.png`;
    case 'international':
      return `${baseUrl}/static/globe-icon.png`;
    case 'peer':
      return `${baseUrl}/static/peer-icon.png`;
    default:
      return `${baseUrl}/static/evaluation-icon.png`;
  }
};

export const IncomingEvaluationEmail = ({
  username,
  projectName,
  projectLink,
  evaluatorType,
  evaluatorName,
  evaluatorCountry,
  expectedCompletionDate,
  evaluationCriteria,
}: IncomingEvaluationEmailProps) => {
  const previewText = `Your project "${projectName}" is being evaluated`;

  const evaluatorDescription = getEvaluatorDescription(
    evaluatorType || 'peer',
    evaluatorName,
    evaluatorCountry
  );

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
              Evaluation In Progress
            </Heading>
            <Text className="text-[14px] text-black leading-[24px]">
              Hello {username},
            </Text>
            <Text className="text-[14px] text-black leading-[24px]">
              Great news! Your project <strong>{projectName}</strong> is
              currently being evaluated by {evaluatorDescription}.
            </Text>
            <Section className="mt-[16px] mb-[16px] text-center">
              <Img
                src={getEvaluatorIcon(evaluatorType || 'peer')}
                width="64"
                height="64"
                alt="Evaluator"
                className="mx-auto my-0"
              />
            </Section>
            <Section className="mt-[16px] rounded bg-[#f6f6f6] p-[16px]">
              <Text className="m-0 text-[14px] text-[#666666] leading-[24px]">
                <strong>Evaluation Details:</strong>
              </Text>
              <Text className="m-0 text-[14px] text-black leading-[24px]">
                • Project: {projectName}
              </Text>
              <Text className="m-0 text-[14px] text-black leading-[24px]">
                • Evaluator Type:{' '}
                {evaluatorType === 'bot'
                  ? 'Automated System'
                  : evaluatorType === 'international'
                    ? 'International Review'
                    : 'Peer Review'}
              </Text>
              {evaluatorCountry && evaluatorType === 'international' && (
                <Text className="m-0 text-[14px] text-black leading-[24px]">
                  • Reviewer Location: {evaluatorCountry}
                </Text>
              )}
              {expectedCompletionDate && (
                <Text className="m-0 text-[14px] text-black leading-[24px]">
                  • Expected Completion: {expectedCompletionDate}
                </Text>
              )}
            </Section>
            {evaluationCriteria && evaluationCriteria.length > 0 && (
              <Section className="mt-[16px]">
                <Text className="text-[14px] text-black leading-[24px]">
                  <strong>Your project will be evaluated on:</strong>
                </Text>
                <ul className="text-[14px] text-black leading-[24px]">
                  {evaluationCriteria.map((criteria, index) => (
                    <li key={index}>{criteria}</li>
                  ))}
                </ul>
              </Section>
            )}
            <Section className="mt-[32px] mb-[32px] text-center">
              <Button
                className="rounded bg-[#000000] px-5 py-3 text-center font-semibold text-[12px] text-white no-underline"
                href={projectLink}
              >
                View Project
              </Button>
            </Section>
            <Text className="text-[14px] text-black leading-[24px]">
              or copy and paste this URL into your browser:{' '}
              <Link href={projectLink} className="text-blue-600 no-underline">
                {projectLink}
              </Link>
            </Text>
            <Hr className="mx-0 my-[26px] w-full border border-[#eaeaea] border-solid" />
            <Text className="text-[#666666] text-[12px] leading-[24px]">
              You will receive another notification once the evaluation is
              complete. In the meantime, please do not make any changes to your
              submitted project.
            </Text>
          </Container>
        </Body>
      </Tailwind>
    </Html>
  );
};

IncomingEvaluationEmail.PreviewProps = {
  username: 'alanturing',
  projectName: 'Enigma Decoder',
  projectLink: 'https://nxtapp.com/projects/enigma-decoder',
  evaluatorType: 'international',
  evaluatorName: 'Maria Garcia',
  evaluatorCountry: 'Spain',
  expectedCompletionDate: 'February 5, 2026',
  evaluationCriteria: [
    'Code quality and organization',
    'Documentation completeness',
    'Innovation and creativity',
    'Technical implementation',
  ],
} as IncomingEvaluationEmailProps;

export default IncomingEvaluationEmail;
