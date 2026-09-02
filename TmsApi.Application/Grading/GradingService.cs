namespace TmsApi.Application.Grading;
public class GradingService
{
    public const decimal DistinctionThreshold = 70m;
    public const decimal PassThreshold = 50m;
    //pure mapping : one score against one maximum score
    // Uses MS's Assessment.MaxScore and the decimal part of enrollment.Grade.
    public GradeLevel CalculateLetterGrade(decimal score, decimal maxScore)
    {
        if(maxScore <=0m || score <0m || score > maxScore)
        return GradeLevel.Invalid;

        var pct = score / maxScore * 100m;
        return pct >= DistinctionThreshold ? GradeLevel.Distinction :
               pct >= PassThreshold ? GradeLevel.Pass : GradeLevel.Fail;
    }
               //Signal decimal path : maps an Enrollment.Grade percentage to a GradeLevel.
               //Enrollment.Grade is nullable per the MS entity; null => Invalid.

               public GradeLevel CalculateFromEnrollmentGrade(decimal? enrollmentGradePercentage)
               {
                   if(enrollmentGradePercentage is null ) 
                   return GradeLevel.Invalid;
                   return CalculateLetterGrade(enrollmentGradePercentage.Value, 100m);
               }
        
    }
