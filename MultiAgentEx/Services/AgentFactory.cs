using Microsoft.Agents.AI;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

namespace MultiAgentEx.Services
{
    public sealed class AgentFactory
    {

        IConfiguration config;
        private readonly string apiKey;
        private readonly string modelName;
        public AgentFactory(IConfiguration config)
        {
            this.config = config;
             apiKey = config["OpenAI:OpenAIKey"]
    ?? throw new InvalidOperationException(
        "OpenAI:ApiKey is not configured.");

            modelName = config["OpenAI:ModelName"]
                ?? throw new InvalidOperationException(
                    "OpenAI:Model is not configured.");
        }


        public AIAgent CreateResearchAgent()
        {

            // Create the OpenAI SDK client.
            var openAIClient = new OpenAIClient(apiKey);

            // Select the model through the Chat Completions client.
            var chatClient = openAIClient.GetChatClient(modelName);


            return chatClient.AsAIAgent(
                instructions: """
                You are a Technical Research Agent.

                Your responsibilities:
                1. Identify the technical questions that must be investigated.
                2. Organize the available information.
                3. Identify facts, assumptions, and missing evidence.
                4. Produce a structured research report.

                Do not pretend that you have inspected source code,
                databases, logs, or documents unless that information
                was actually provided or retrieved through an available tool.

                Do not present assumptions as confirmed facts.

                Output sections:
                - Research Question
                - Available Evidence
                - Relevant Technical Concepts
                - Missing Information
                - Research Summary
                """,
                name: "ResearchAgent");
        }

        public AIAgent CreateAnalysisAgent()
        {
            // Create the OpenAI SDK client.
            var openAIClient = new OpenAIClient(apiKey);

            // Select the model through the Chat Completions client.
            var chatClient = openAIClient.GetChatClient(modelName);

            return chatClient.AsAIAgent(
                instructions: """
                You are a Senior Software Engineering Analysis Agent.

                You receive a research report from another agent.

                Your responsibilities:
                1. Analyze the supplied evidence.
                2. Identify plausible technical causes or problems.
                3. Explain the reasoning behind each finding.
                4. Recommend practical improvements.
                5. Separate confirmed findings from hypotheses.

                Do not claim that a potential problem is confirmed
                unless the supplied evidence supports that conclusion.

                Do not invent benchmark results, test results,
                source-code findings, or production measurements.

                Output sections:
                - Analysis
                - Potential Issues
                - Evidence and Reasoning
                - Recommended Improvements
                - Limitations
                """,
                name: "AnalysisAgent");
        }

        public AIAgent CreateReviewAgent()
        {
            // Create the OpenAI SDK client.
            var openAIClient = new OpenAIClient(apiKey);

            // Select the model through the Chat Completions client.
            var chatClient = openAIClient.GetChatClient(modelName);

            return chatClient.AsAIAgent(
                instructions: """
                You are an Independent Technical Review Agent.

                You receive a research report and a technical analysis.

                Your responsibilities:
                1. Check whether the conclusions follow from the evidence.
                2. Identify unsupported or exaggerated claims.
                3. Identify missing risks and important limitations.
                4. Check whether recommendations address the question.
                5. Produce a clear final answer for a software developer.

                Do not blindly approve the analysis.
                Correct unsupported statements and qualify uncertainty.
                Do not invent evidence to fill gaps.

                Structure the final answer as:
                - Executive Summary
                - Key Findings
                - Recommended Actions
                - Additional Information Needed
                """,
                name: "ReviewAgent");
        }
    }

}
