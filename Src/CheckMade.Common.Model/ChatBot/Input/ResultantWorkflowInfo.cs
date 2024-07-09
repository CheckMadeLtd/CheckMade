namespace CheckMade.Common.Model.ChatBot.Input;

public record ResultantWorkflowInfo(
    string WorkflowId,
    long InState)
{
    public ResultantWorkflowInfo(
            string workflowId,
            Enum inState)
        : this(
            workflowId,
            Convert.ToInt64(inState))
    {
    }
}