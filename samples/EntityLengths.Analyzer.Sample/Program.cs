using EntityLengths.Analyzer.Sample.Entities;

var ctd = new ColumnTypeDefinitionUser
{
    Name = "John Doe0",
    Name1 = "John Doe1",
    Name2 = "John Doe2",
};

var da = new DataAnnotationUser { Name = "John Doe3", Surname = "John Doe4" };

var dc = new DbContextUser
{
    Description = "John Doe6",
    Name2 = "John Doe7",
    Description2 = "John Doe8",
    Name = "John5",
    Age = 30,
};

var fu = new FluentUser();
fu.Name = "John Doe";
fu.Description = "John Doe";
