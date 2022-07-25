using Microsoft.AspNetCore.Mvc;
using static PetsFunctions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapGet("pets", () =>
    GetPets()
    .Match<IResult>(
        Some: ps => Results.Ok(new { Pets = ps }),
        None: Results.NotFound()));

app.MapGet("pets/{petId}", (string petId) =>
    GetPet(petId)
    .Match<IResult>(
        Some: p => Results.Ok(p),
        None: Results.NotFound()));

app.MapGet("pets/{petId}/needs", (string petId) =>
    GetNeeds(petId)
    .Match<IResult>(
        Some: ns => Results.Ok(new { Needs = ns }),
        None: Results.NotFound()));


app.MapPost("pets/{petId}/activities", (string petId, [FromBody] Activity activity) =>
    LogActivity(petId, activity)
    .Match<IResult>(
        Succ: a => Results.Created("activities", a),
        Fail: e =>
        {
            Console.WriteLine(e);
            return Results.StatusCode(500);
        }));

app.MapGet("pets/{petId}/activities", (string petId) => 
    GetActivities(petId)
    .Match<IResult>(
        Some: acs => Results.Ok(new { Activities = acs }),
        None: Results.NotFound()));

app.Run();
