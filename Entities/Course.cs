namespace Josi_TmsApi.Entities;
public class Course
{
    public int Id { get; set; }
    //surrogate primary key -internal , used by forign kes
    public required string Code { get; set; }
    //natural key -human readable,(uniquess configured in session  2)
    public required string Title { get; set; }
    public int MaxCapacity { get; set; }
    //navigation property many to many relationship
    public ICollection <Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection <Assessment> Assessments { get; set; } = new List<Assessment>();
    public ICollection <Certificate> Certificates{ get; set; } = new List<Certificate>();
}