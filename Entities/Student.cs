namespace Josi_TmsApi.Entities
{
    public class Student
    {
       public int Id {get;set;}
       //surrogate primary key -internal ,used by foreign keys
       public required string RegistrationNumber {get;set;}
       //natural key -human readable,(uniquess configured in session 2)
       public required string Name {get;set;}
<<<<<<< HEAD
<<<<<<< HEAD
  
       public decimal GPA {get;set;}
      
=======
       public decimal GPA {get;set;}
>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
=======
  
       public decimal GPA {get;set;}
      
>>>>>>> 70789a3ac36423b19b24234c54947951ff4298e8
       public bool IsActive {get;set;}= true;
       //navigation property for many to many relationship 
       public ICollection <Enrollment> Enrollments {get;set;} = new List<Enrollment>();
       public ICollection <Certificate> Certificates {get;set;} = new List<Certificate>();

    }
}