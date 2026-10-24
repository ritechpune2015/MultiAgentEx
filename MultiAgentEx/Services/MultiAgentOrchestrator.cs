using Microsoft.Agents.AI;
using MultiAgentEx.Models;
using System.Diagnostics;

namespace MultiAgentEx.Services
{
    public sealed class MultiAgentOrchestrator
    {
        private readonly AgentFactory _agentFactory;
        private readonly ILogger<MultiAgentOrchestrator> _logger;

        public MultiAgentOrchestrator(
            AgentFactory agentFactory,
            ILogger<MultiAgentOrchestrator> logger)
        {
            _agentFactory = agentFactory;
            _logger = logger;
        }

        public async Task<MultiAgentResult> ExecuteAsync(
            string question,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException(
                    "A question is required.",
                    nameof(question));
            }

            var stopwatch = Stopwatch.StartNew();

            // Create separate agents for this execution.
            AIAgent researchAgent =
                _agentFactory.CreateResearchAgent();

            AIAgent analysisAgent =
                _agentFactory.CreateAnalysisAgent();

            AIAgent reviewAgent =
                _agentFactory.CreateReviewAgent();

            try
            {
                // STEP 1: Research
                _logger.LogInformation(
                    "Research Agent started.");

                var researchPrompt = $"""
                Investigate the following technical question.

                User question:
                {question}

                Produce a structured research report.
                Clearly separate known facts, assumptions,
                and information that is missing.
                """;

                var researchResponse = await researchAgent.RunAsync(
                    researchPrompt,
                    cancellationToken: cancellationToken);

                string researchReport = researchResponse.ToString();

                if (string.IsNullOrWhiteSpace(researchReport))
                {
                    throw new InvalidOperationException(
                        "The Research Agent returned an empty response.");
                }

                _logger.LogInformation(
                    "Research Agent completed.");

                cancellationToken.ThrowIfCancellationRequested();

                // STEP 2: Analysis
                _logger.LogInformation(
                    "Analysis Agent started.");

                var analysisPrompt = $"""
                Analyze the user's question using the research
                report supplied below.

                Original question:
                {question}

                Research report:
                <research_report>
                {researchReport}
                </research_report>

                Identify plausible problems, explain your reasoning,
                recommend practical improvements, and identify
                limitations in the available evidence.

                Do not treat an assumption in the research report
                as a confirmed fact.
                """;

                var analysisResponse = await analysisAgent.RunAsync(
                    analysisPrompt,
                    cancellationToken: cancellationToken);

                string analysisReport = analysisResponse.ToString();

                if (string.IsNullOrWhiteSpace(analysisReport))
                {
                    throw new InvalidOperationException(
                        "The Analysis Agent returned an empty response.");
                }

                _logger.LogInformation(
                    "Analysis Agent completed.");

                cancellationToken.ThrowIfCancellationRequested();

                // STEP 3: Review
                _logger.LogInformation(
                    "Review Agent started.");

                var reviewPrompt = $"""
                Review the technical analysis and prepare
                the final answer to the user's original question.

                Original question:
                {question}

                Research report:
                <research_report>
                {researchReport}
                </research_report>

                Technical analysis:
                <analysis_report>
                {analysisReport}
                </analysis_report>

                Check whether the conclusions are supported by
                the supplied evidence. Correct unsupported claims,
                preserve important uncertainty, and prioritize
                actionable recommendations.

                Do not invent additional evidence.
                Produce a concise, well-structured final answer.
                """;

                var reviewResponse = await reviewAgent.RunAsync(
                    reviewPrompt,
                    cancellationToken: cancellationToken);

                string finalAnswer = reviewResponse.ToString();

                if (string.IsNullOrWhiteSpace(finalAnswer))
                {
                    throw new InvalidOperationException(
                        "The Review Agent returned an empty response.");
                }

                _logger.LogInformation(
                    "Review Agent completed.");

                stopwatch.Stop();

                return new MultiAgentResult
                {
                    ResearchReport = researchReport,
                    AnalysisReport = analysisReport,
                    FinalAnswer = finalAnswer,
                    Duration = stopwatch.Elapsed
                };
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "Multi-agent execution was cancelled.");

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Multi-agent execution failed.");

                throw;
            }
        }
    }

}
