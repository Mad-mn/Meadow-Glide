using System.Collections.Generic;

namespace Feature.TransactionModule.Scripts
{
    public class TransactionResult
    {
        public bool Success;
        public TransactionFailureReason Reason;
        public string Message;
        public List<ResourceAmount> AppliedRewards;

        public static TransactionResult Ok(List<ResourceAmount> rewards = null)
        {
            return new TransactionResult
            {
                Success = true,
                Reason = TransactionFailureReason.None,
                Message = string.Empty,
                AppliedRewards = rewards ?? new List<ResourceAmount>()
            };
        }

        public static TransactionResult Fail(TransactionFailureReason reason, string message = "")
        {
            return new TransactionResult
            {
                Success = false,
                Reason = reason,
                Message = message,
                AppliedRewards = new List<ResourceAmount>()
            };
        }
    }
}
