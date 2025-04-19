// Implemented by Hudson Collier and Ian Kerr
// Last Edited: April 16, 2025

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from c in db.Courses
                        join
                        cl in db.Classes on c.CourseId equals cl.CourseId
                        join
                        e in db.Enrolleds on cl.ClassId equals e.ClassId
                        join
                        s in db.Students on e.UId equals s.UId
                        where c.Subject == subject && c.Num == num && cl.Season == season && cl.Semester == year
                        select new
                        {
                            fname = s.FirstName,
                            lname = s.LastName,
                            uid = s.UId,
                            dob = s.Dob,
                            grade = e.Grade,
                        };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if(category == null)
            {
                var query = from c in db.Courses
                            join
                            cl in db.Classes on c.CourseId equals cl.CourseId
                            join
                            ac in db.AssignmentCategories on cl.ClassId equals ac.ClassId
                            join
                            a in db.Assignments on ac.CategoryId equals a.CategoryId
                            where subject == c.Subject && num == c.Num && season == cl.Season && year == cl.Semester
                            select new
                            {
                                aname = a.Name,
                                cname = ac.Name,
                                due = a.Due,
                                submissions = a.Submissions.Count,
                            };

                return Json(query.ToArray());
            }


            var query2 = from c in db.Courses
                        join
                        cl in db.Classes on c.CourseId equals cl.CourseId
                        join
                        ac in db.AssignmentCategories on cl.ClassId equals ac.ClassId
                        join
                        a in db.Assignments on ac.CategoryId equals a.CategoryId
                        where subject == c.Subject && num == c.Num && season == cl.Season && year == cl.Semester && category == ac.Name
                        select new
                        {
                            aname = a.Name,
                            cname = ac.Name,
                            due = a.Due,
                            submissions = a.Submissions.Count,
                        };
            return Json(query2.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from c in db.Courses
                        join
                        cl in db.Classes on c.CourseId equals cl.CourseId
                        join
                        ac in db.AssignmentCategories on cl.ClassId equals ac.ClassId
                        where c.Subject == subject && num == c.Num && cl.Season == season && cl.Semester == year
                        select new
                        {
                            name = ac.Name,
                            weight = ac.Weight,

                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var query = from c in db.Courses
                        join
                        cl in db.Classes on c.CourseId equals cl.CourseId
                        
                        where c.Subject == subject && c.Num == num && cl.Season == season && cl.Semester == year
                        select new
                        {
                            classId = cl.ClassId,
                            
                        };
            if (query.Any())
            {
                var categoryQuery = from q in query
                                    join
                                    ac in db.AssignmentCategories on q.classId equals ac.ClassId
                                    where ac.Name == category
                                    select ac;
                if(categoryQuery.Any())
                    return Json(new { success = false });



                AssignmentCategory newac = new AssignmentCategory();
                newac.Name = category;
                newac.Weight = (uint)catweight;
                newac.ClassId = query.First().classId;
                db.AssignmentCategories.Add(newac);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            // Checks if the assignment category exists
            var categoryQuery = from c in db.Courses
                        join
                        cl in db.Classes on c.CourseId equals cl.CourseId
                        join
                        ac in db.AssignmentCategories on cl.ClassId equals ac.ClassId
                        where c.Subject == subject && c.Num == num && cl.Season == season && cl.Semester == year && ac.Name == category
                        select ac;

            // Sets to null if the category does not exists
            var assignCategory = categoryQuery.FirstOrDefault();

            if (assignCategory == null || db.Assignments.Any(assign => assign.CategoryId == assignCategory.CategoryId && assign.Name == asgname))
            {
                return Json(new { success = false }); 
            }

            Assignment assign = new Assignment();
            assign.Name = asgname;
            assign.Contents = asgcontents;
            assign.Due = asgdue;
            assign.CategoryId = assignCategory.CategoryId;
            assign.MaxPoints = (uint)asgpoints;
            db.Assignments.Add(assign);
            db.SaveChanges();


            var allStudentsInClass = from e in db.Enrolleds
                                     join cl in db.Classes on e.ClassId equals cl.ClassId
                                     join c in db.Courses on cl.CourseId equals c.CourseId
                                     where c.Subject == subject && c.Num == num && cl.Season == season && cl.Semester == year
                                     select new { 
                                     uid = e.UId
                                     };

            foreach(var student in allStudentsInClass.ToList())
            {
                UpdateGrades(subject, num, season, year, category, asgname, student.uid);
            }

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from c in db.Courses
                        join
                        cl in db.Classes on c.CourseId equals cl.CourseId
                        join
                        ac in db.AssignmentCategories on cl.ClassId equals ac.ClassId
                        join a in db.Assignments on ac.CategoryId equals a.CategoryId
                        join s in db.Submissions on a.AssignId equals s.AssignId
                        join st in db.Students on s.UId equals st.UId
                        where c.Subject == subject && c.Num == num && cl.Season == season && cl.Semester == year && ac.Name == category && a.Name == asgname
                        select new
                        {
                            fname = st.FirstName,
                            lname = st.LastName,
                            uid = st.UId,
                            time = s.Time,
                            score = s.Score,
                        };
            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var query = from c in db.Courses
                        join cl in db.Classes
                        on c.CourseId equals cl.CourseId
                        join
                        ac in db.AssignmentCategories on cl.ClassId equals ac.ClassId
                        join
                        a in db.Assignments on ac.CategoryId equals a.CategoryId
                        join
                        s in db.Submissions on a.AssignId equals s.AssignId
                        where subject == c.Subject && num == c.Num && season == cl.Season && year == cl.Semester && category == ac.Name && a.Name == asgname && uid == s.UId
                        select s;
                        

            if (query.Any())
            {
                query.First().Score = (uint)score;
                db.SaveChanges();

                UpdateGrades(subject, num, season, year, category, asgname, uid);
               
                return Json(new { success = true });
            }
            return Json(new { success = false });

        }

        /// <summary>
        /// Updates the given students grades
        /// </summary>
        /// <param name="subject">Course subject</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The class season</param>
        /// <param name="year">The class year</param>
        /// <param name="category">The assignment category</param>
        /// <param name="asgname">The assignment name</param>
        /// <param name="uid">The students UID</param>
        private void UpdateGrades(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            uint classID = GetClassID(subject, num, season, year);

            List<uint> nonEmptyCateogryIDs = retrievesAssignmentCategories(uid, classID);
            double totalscaled = 0;
            int totalWeights = 0;
            foreach (var categoryID in nonEmptyCateogryIDs)
            {
                var result = retrievesAssignments(uid, categoryID, classID);
                totalscaled += result[0];
                totalWeights += (int)result[1];

            }

            double scalingFactor = 100.0 / totalWeights;
            double totalPercentage = totalscaled * scalingFactor;

            InsertIntoEnrolled(classID, uid, totalPercentage);
        }

        /// <summary>
        /// Private helper method in order to insert a students grades into the enrolled table
        /// </summary>
        /// <param name="classID">The class id</param>
        /// <param name="uid">Students UID</param>
        /// <param name="finalGrade">Final grade for that given class</param>
        private void InsertIntoEnrolled(uint classID, string uid, double finalGrade)
        { 
            var enrollment = db.Enrolleds
            .FirstOrDefault(e => e.ClassId == classID && e.UId == uid);

            if (enrollment != null)
            {
                enrollment.Grade = ConvertFinalGradeToLetterGrade(finalGrade);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Heplper method in order to get the id for a given class
        /// </summary>
        /// <param name="subject">The course subject</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The class season</param>
        /// <param name="year">The semester</param>
        /// <returns>The given class ID</returns>
        private uint GetClassID(string subject, int num, string season, int year)
        {
            var query = from c in db.Courses
                        join cl in db.Classes
                        on c.CourseId equals cl.CourseId
                        where subject == c.Subject && num == c.Num && season == cl.Season && year == cl.Semester
                        select new
                        {
                            classID = cl.ClassId
                        };

            return query.First().classID;
        }

        /// <summary>
        /// Helper method in order to convert the final class grade into a letter grade
        /// </summary>
        /// <param name="finalGrade">Final grade for a given class</param>
        /// <returns>The letter grade as a string</returns>
        private string ConvertFinalGradeToLetterGrade(double finalGrade)
        {
            if (finalGrade >= 93) return "A";
            else if (finalGrade >= 90) return "A-";
            else if (finalGrade >= 87) return "B+";
            else if (finalGrade >= 83) return "B";
            else if (finalGrade >= 80) return "B-";
            else if (finalGrade >= 77) return "C+";
            else if (finalGrade >= 73) return "C";
            else if (finalGrade >= 70) return "C-";
            else if (finalGrade >= 67) return "D+";
            else if (finalGrade >= 63) return "D";
            else if (finalGrade >= 60) return "D-";
            else return "E";
        }

        /// <summary>
        /// Retrieves the non empty assignment categories
        /// </summary>
        /// <param name="uid">Students uid</param>
        /// <param name="classID">id for the given class</param>
        /// <returns>A list containing the assignmenet category ID's</returns>
        private List<uint> retrievesAssignmentCategories(string uid, uint classID)
        {
            var query = from cl in db.Classes
                        join ac in db.AssignmentCategories on cl.ClassId equals ac.ClassId
                        join e in db.Enrolleds on cl.ClassId equals e.ClassId
                        where cl.ClassId == classID && e.UId == uid
                        select new
                        {
                            ac.CategoryId,

                        };
            List<uint> returnList = new List<uint>();

            foreach(var category in query.ToList())
            {
                returnList.Add(category.CategoryId);
            }

            return returnList;
        }

        /// <summary>
        /// Retrieves all the assignments for a given category
        /// </summary>
        /// <param name="uid">The students uid</param>
        /// <param name="categoryID">The assignment cateogry</param>
        /// <param name="classID">The id for the given class</param>
        /// <returns>An array containing two values, the first being the scaled total grade contribution and 
        /// the second is total weight of assignment categoriess</returns>
        private double[] retrievesAssignments(string uid, uint categoryID, uint classID)
        {
            var query = from ac in db.AssignmentCategories
                        join a in db.Assignments on ac.CategoryId equals a.CategoryId
                        join e in db.Enrolleds on ac.ClassId equals e.ClassId
                        where ac.CategoryId == categoryID && ac.ClassId == classID && e.UId == uid
                        select new
                        {
                            assignID = a.AssignId,
                            maxPoints = a.MaxPoints,
                            weight = ac.Weight
                        };

            var assignments = query.ToList();

            int totalPointsEarned = 0;
            int totalMaxPoints = 0;

            foreach (var assignment in assignments)
            {
                // Get the submission if it exists
                var submission = db.Submissions
                                   .FirstOrDefault(s => s.AssignId == assignment.assignID && s.UId == uid);
                uint score;
                if (submission == null)
                    score = 0;
                else
                    score = submission.Score ?? 0;

                totalPointsEarned += (int)score;
                totalMaxPoints += (int)assignment.maxPoints;
            }

            if (totalMaxPoints == 0)
                return new double[2] { 0, 0 };

            double percentage = (double)totalPointsEarned / totalMaxPoints;
            return new double[2] { percentage * assignments.First().weight, assignments.First().weight };
        }



        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
                        var query = from cl in db.Classes
                        join c in db.Courses on cl.CourseId equals c.CourseId
                        join p in db.Professors on cl.Teacher equals p.UId 
                        where p.UId == uid
                        select new
                        {
                            subject = c.Subject,
                            number = c.Num,
                            name = c.Name,
                            season = cl.Season,
                            year = cl.Semester,
                        };
            return Json(query.ToArray());
        }
        /*******End code to modify********/
    }
}

