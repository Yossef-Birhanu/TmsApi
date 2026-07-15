namespace Josi_TmsApi.Dtos;
public record StudentResponseDto(
    int Id,
    string RegistrationNumber,
    string Name,
    int EnrollmentCount
);