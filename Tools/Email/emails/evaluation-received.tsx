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

interface EvaluationReceivedEmailProps {
  username?: string;
  projectName?: string;
  evaluatorType?: EvaluatorType;
  evaluatorName?: string;
  evaluatorCountry?: string;
  overallScore?: number;
  maxScore?: number;
  evaluationSummary?: string;
  strengths?: string[];
  improvements?: string[];
  evaluationLink?: string;
  completedDate?: string;
}

const baseUrl = process.env.VERCEL_URL
  ? `https://${process.env.VERCEL_URL}`
  : '';

const getEvaluatorLabel = (
  type: EvaluatorType,
  name?: string,
  country?: string
): string => {
  switch (type) {
    case 'bot':
      return 'Automated Evaluation System';
    case 'international':
      return `International Reviewer${country ? ` (${country})` : ''}`;
    case 'peer':
      return name ? `${name} (Peer)` : 'Peer Reviewer';
    default:
      return 'Reviewer';
  }
};

export const EvaluationReceivedEmail = ({
  username,
  projectName,
  evaluatorType,
  evaluatorName,
  evaluatorCountry,
  overallScore,
  maxScore,
  evaluationSummary,
  strengths,
  improvements,
  evaluationLink,
  completedDate,
}: EvaluationReceivedEmailProps) => {
  const previewText = `Evaluation completed for "${projectName}"`;

  const evaluatorLabel = getEvaluatorLabel(
    evaluatorType || 'peer',
    evaluatorName,
    evaluatorCountry
  );

  const scorePercentage =
    overallScore && maxScore ? Math.round((overallScore / maxScore) * 100) : 0;

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
              Evaluation Complete! 🎉
            </Heading>
            <Text className="text-[14px] text-black leading-[24px]">
              Hello {username},
            </Text>
            <Text className="text-[14px] text-black leading-[24px]">
              Your project <strong>{projectName}</strong> has been evaluated by{' '}
              <strong>{evaluatorLabel}</strong>.
            </Text>
            {overallScore !== undefined && maxScore !== undefined && (
              <Section className="mt-[16px] mb-[16px] text-center">
                <div
                  className="mx-auto rounded-full bg-[#f6f6f6] p-[20px]"
                  style={{ width: '120px', height: '120px' }}
                >
                  <Text className="m-0 font-bold text-[32px] text-black">
                    {overallScore}/{maxScore}
                  </Text>
                  <Text className="m-0 text-[14px] text-[#666666]">
                    ({scorePercentage}%)
                  </Text>
                </div>
              </Section>
            )}
            <Section className="mt-[16px] rounded bg-[#f6f6f6] p-[16px]">
              <Text className="m-0 text-[14px] text-[#666666] leading-[24px]">
                <strong>Evaluation Summary:</strong>
              </Text>
              <Text className="m-0 text-[14px] text-black leading-[24px]">
                {evaluationSummary}
              </Text>
              {completedDate && (
                <Text className="m-0 mt-[8px] text-[12px] text-[#666666] leading-[24px]">
                  Completed on: {completedDate}
                </Text>
              )}
            </Section>
            {strengths && strengths.length > 0 && (
              <Section className="mt-[16px]">
                <Text className="text-[14px] text-black leading-[24px]">
                  <strong>✅ Strengths:</strong>
                </Text>
                <ul className="text-[14px] text-black leading-[24px]">
                  {strengths.map((strength, index) => (
                    <li key={index}>{strength}</li>
                  ))}
                </ul>
              </Section>
            )}
            {improvements && improvements.length > 0 && (
              <Section className="mt-[16px]">
                <Text className="text-[14px] text-black leading-[24px]">
                  <strong>💡 Areas for Improvement:</strong>
                </Text>
                <ul className="text-[14px] text-black leading-[24px]">
                  {improvements.map((improvement, index) => (
                    <li key={index}>{improvement}</li>
                  ))}
                </ul>
              </Section>
            )}
            <Section className="mt-[32px] mb-[32px] text-center">
              <Button
                className="rounded bg-[#000000] px-5 py-3 text-center font-semibold text-[12px] text-white no-underline"
                href={evaluationLink}
              >
                View Full Evaluation
              </Button>
            </Section>
            <Text className="text-[14px] text-black leading-[24px]">
              or copy and paste this URL into your browser:{' '}
              <Link
                href={evaluationLink}
                className="text-blue-600 no-underline"
              >
                {evaluationLink}
              </Link>
            </Text>
            <Hr className="mx-0 my-[26px] w-full border border-[#eaeaea] border-solid" />
            <Text className="text-[#666666] text-[12px] leading-[24px]">
              Congratulations on completing your evaluation! Use this feedback
              to improve your future projects. If you have any questions about
              your evaluation, please contact our support team.
            </Text>
          </Container>
        </Body>
      </Tailwind>
    </Html>
  );
};

EvaluationReceivedEmail.PreviewProps = {
  username: 'alanturing',
  projectName: 'Enigma Decoder',
  evaluatorType: 'international',
  evaluatorName: 'Maria Garcia',
  evaluatorCountry: 'Spain',
  overallScore: 85,
  maxScore: 100,
  evaluationSummary:
    'Excellent work on the Enigma Decoder project! The implementation demonstrates strong problem-solving skills and creative thinking. The code is well-organized and documented.',
  strengths: [
    'Clean and maintainable code structure',
    'Comprehensive documentation',
    'Innovative approach to the problem',
    'Good use of design patterns',
  ],
  improvements: [
    'Could add more unit tests for edge cases',
    'Consider optimizing the decryption algorithm',
    'Add error handling for invalid inputs',
  ],
  evaluationLink: 'https://nxtapp.com/evaluations/abc123',
  completedDate: 'February 1, 2026',
} as EvaluationReceivedEmailProps;

export default EvaluationReceivedEmail;
