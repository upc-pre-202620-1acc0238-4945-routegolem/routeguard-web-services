namespace RouteGuard.Platform.Stakeholder.Domain.Model;

public enum StakeholderError
{
    InvalidStakeholderData,
    ParentNotFound,
    DriverNotFound,
    StudentGroupNotFound,
    InvalidStudentGroupState,
    DatabaseError,
    InternalServerError
}
