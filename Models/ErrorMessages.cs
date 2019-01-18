namespace restapi.Models
{
    public class InvalidStateError
    {
        public int ErrorCode { get => 100; }

        public string Message { get => "Transition not valid for current state"; }
    }
    public class InvalidUser
    {
        public int ErrorCode { get => 401; }

        public string Message { get => "User is not valid"; }
    }
    public class NotAuthorized
    {
        public int ErrorCode { get => 403; }

        public string Message { get => "This user is not permitted"; }
    }

    public class EmptyTimecardError
    {
        public int ErrorCode { get => 101; }

        public string Message { get => "Unable to submit timecard with no lines"; }
    }

    public class MissingTransitionError
    {
        public int ErrorCode { get => 102; }

        public string Message { get => "No state transition of requested type present in timecard"; }
    }
}