namespace RouteGuard.Platform.StakeholderAssetManagement.Domain.Model;

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
