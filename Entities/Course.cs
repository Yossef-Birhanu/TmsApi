namespace Josi_TmsApi.Entities;
<<<<<<< HEAD


=======
>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
public class Course
{
    public int Id { get; set; }
    //surrogate primary key -internal , used by forign kes
    public required string Code { get; set; }
    //natural key -human readable,(uniquess configured in session  2)
    public required string Title { get; set; }
<<<<<<< HEAD
    public int MaxCapacity { get; set; }
    //navigation property many to many relationship
   // public ICollection <Enrollment> Enrollments { get; set; } = new List<Enrollment>();
   public ICollection <Enrollment> Enrollments {get; set;}=[];

   //Confirm that Course configuration has the rules for both properties configured
   
    public ICollection <Assessment> Assessments { get; set; } = new List<Assessment>();
    public ICollection <Certificate> Certificates{ get; set; } = new List<Certificate>();
}
=======
    public int Capacity { get; set; }
    //navigation property many to many relationship
    public ICollection <Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection <Assessment> Assessments { get; set; } = new List<Assessment>();
    public ICollection <Certificate> Certificates{ get; set; } = new List<Certificate>();
}
>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
