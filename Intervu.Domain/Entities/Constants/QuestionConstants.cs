using System.ComponentModel;

namespace Intervu.Domain.Entities.Constants.QuestionConstants
{
    public enum QuestionStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Removed = 4
    }

    public enum QuestionReportStatus
    {
        Pending = 1,
        Resolved = 2,
        Dismissed = 3
    }

    public enum ResolutionAction
    {
        [Description("No Action")]
        NoAction = 0,
        [Description("Deactivate Question")]
        DeactivateQuestion = 1,
        [Description("Edit Question")]
        EditQuestion = 2
    }

    public enum Role
    {
        [Description("Product Manager")]
        ProductManager = 1,
        [Description("Software Engineer")]
        SoftwareEngineer = 2,
        [Description("Data Engineer")]
        DataEngineer = 3,
        [Description("Data Scientist")]
        DataScientist = 4,
        [Description("Technical Program Manager")]
        TechnicalProgramManager = 5,
        [Description("Backend Engineer")]
        BackendEngineer = 6,
        [Description("Frontend Engineer")]
        FrontendEngineer = 7,
        [Description("Full Stack Engineer")]
        FullStackEngineer = 8,
        [Description("Mobile Engineer")]
        MobileEngineer = 9,
        [Description("DevOps Engineer")]
        DevOpsEngineer = 10,
        [Description("QA Engineer")]
        QAEngineer = 11,
        [Description("Machine Learning Engineer")]
        MachineLearningEngineer = 12,
        [Description("Security Engineer")]
        SecurityEngineer = 13,
        [Description("Cloud Engineer")]
        CloudEngineer = 14,
        [Description("UI/UX Designer")]
        UIUXDesigner = 15,
        [Description("Business Analyst")]
        BusinessAnalyst = 16,
        [Description("Solution Architect")]
        SolutionArchitect = 17
    }

    public enum QuestionCategory
    {
        Behavioral = 1,
        Technical = 2,
        [Description("System Design")]
        SystemDesign = 3,
        [Description("Case Study")]
        CaseStudy = 4,
        Other = 5,
        Coding = 6,
        Database = 7,
        Networking = 8,
        OOP = 9,
        Algorithms = 10,
        [Description("Data Structures")]
        DataStructures = 11,
        Concurrency = 12,
        [Description("Distributed Systems")]
        DistributedSystems = 13,
        Cloud = 14,
        DevOps = 15
    }

    public enum ExperienceLevel
    {
        Intern = 0,
        Junior = 1,
        Middle = 2,
        Senior = 3,
        Lead = 4,
        Manager = 5,
        Director = 6,
        Expert = 7
    }

    public enum SortOption
    {
        Hot = 1,
        New = 2,
        Top = 3
    }

    public enum InterviewRound
    {
        [Description("Phone Screen")]
        PhoneScreen = 1,
        [Description("Technical Screen")]
        TechnicalScreen = 2,
        [Description("Take Home")]
        TakeHome = 3,
        [Description("Onsite / Final Round")]
        OnsiteFinalRound = 4,
        Other = 5,
        [Description("HR Round")]
        HRRound = 6,
        [Description("Coding Challenge")]
        CodingChallenge = 7,
        [Description("Live Coding")]
        LiveCoding = 8,
        [Description("System Design Round")]
        SystemDesignRound = 9,
        [Description("Behavioral Round")]
        BehavioralRound = 10,
        [Description("Managerial Round")]
        ManagerialRound = 11
    }

    public enum QuestionType
    {
        Behavioral = 1,
        Technical = 2,
        [Description("System Design")]
        SystemDesign = 3,
        [Description("Case Study")]
        CaseStudy = 4,
        Other = 5,
        Coding = 6,
        Database = 7,
        Networking = 8,
        OOP = 9,
        Algorithms = 10,
        [Description("Data Structures")]
        DataStructures = 11,
        Concurrency = 12,
        [Description("Distributed Systems")]
        DistributedSystems = 13,
        Cloud = 14,
        DevOps = 15
    }
}