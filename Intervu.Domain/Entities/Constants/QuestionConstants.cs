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
        Reviewed = 2,
        Dismissed = 3
    }

    public enum Role
    {
        ProductManager = 1,
        SoftwareEngineer = 2,
        DataEngineer = 3,
        DataScientist = 4,
        TechnicalProgramManager = 5,
        BackendEngineer = 6,
        FrontendEngineer = 7,
        FullStackEngineer = 8,
        MobileEngineer = 9,
        DevOpsEngineer = 10,
        QAEngineer = 11,
        MachineLearningEngineer = 12,
        SecurityEngineer = 13,
        CloudEngineer = 14,
        UIUXDesigner = 15,
        BusinessAnalyst = 16,
        SolutionArchitect = 17
    }

    public enum QuestionCategory
    {
        Behavioral = 1,
        Technical = 2,
        SystemDesign = 3,
        CaseStudy = 4,
        Other = 5,
        Coding = 6,
        Database = 7,
        Networking = 8,
        OOP = 9,
        Algorithms = 10,
        DataStructures = 11,
        Concurrency = 12,
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
        PhoneScreen = 1,
        TechnicalScreen = 2,
        TakeHome = 3,
        OnsiteFinalRound = 4,
        Other = 5,
        HRRound = 6,
        CodingChallenge = 7,
        LiveCoding = 8,
        SystemDesignRound = 9,
        BehavioralRound = 10,
        ManagerialRound = 11
    }

    public enum QuestionType
    {
        Behavioral = 1,
        Technical = 2,
        SystemDesign = 3,
        CaseStudy = 4,
        Other = 5,
        Coding = 6,
        Database = 7,
        Networking = 8,
        OOP = 9,
        Algorithms = 10,
        DataStructures = 11,
        Concurrency = 12,
        DistributedSystems = 13,
        Cloud = 14,
        DevOps = 15
    }
}