
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string is missing.");

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
// Serve the frontend files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Initialize the students table.
await using (var connection = new MySqlConnection(connectionString))
{
    await connection.OpenAsync();

    const string createTableSql = """
        CREATE TABLE IF NOT EXISTS students (
            id INT AUTO_INCREMENT PRIMARY KEY,
            name VARCHAR(100) NOT NULL,
            dob DATE NOT NULL,
            gender VARCHAR(20) NULL,
            email VARCHAR(150) NULL,
            phone VARCHAR(20) NULL,
            address VARCHAR(255) NULL,
            grade VARCHAR(20) NULL
        );
        """;

    await using var command = new MySqlCommand(createTableSql, connection);
    await command.ExecuteNonQueryAsync();
}

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

// Create student
app.MapPost("/api/students", async (StudentRequest student) =>
{
    if (string.IsNullOrWhiteSpace(student.Name) || student.Name.Length > 100)
        return Results.BadRequest("Student name is required and must not exceed 100 characters.");

    if (student.Dob == default)
        return Results.BadRequest("Date of birth is required.");

    const string sql = """
        INSERT INTO students (name, dob, gender, email, phone, address, grade)
        VALUES (@name, @dob, @gender, @email, @phone, @address, @grade);
        """;

    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);

    command.Parameters.AddWithValue("@name", student.Name.Trim());
    command.Parameters.AddWithValue("@dob", student.Dob.ToDateTime(TimeOnly.MinValue));
    command.Parameters.AddWithValue("@gender", (object?)student.Gender ?? DBNull.Value);
    command.Parameters.AddWithValue("@email", (object?)student.Email ?? DBNull.Value);
    command.Parameters.AddWithValue("@phone", (object?)student.Phone ?? DBNull.Value);
    command.Parameters.AddWithValue("@address", (object?)student.Address ?? DBNull.Value);
    command.Parameters.AddWithValue("@grade", (object?)student.Grade ?? DBNull.Value);

    await command.ExecuteNonQueryAsync();

    return Results.Created($"/api/students/{command.LastInsertedId}",
        new { id = command.LastInsertedId, message = "Student created successfully." });
});

// Read all students
app.MapGet("/api/students", async () =>
{
    const string sql = "SELECT * FROM students ORDER BY id;";

    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);
    await using var reader = await command.ExecuteReaderAsync();

    var students = new List<StudentResponse>();

    while (await reader.ReadAsync())
    {
        students.Add(ReadStudent(reader));
    }

    return Results.Ok(students);
});

// Read one student
app.MapGet("/api/students/{id:int}", async (int id) =>
{
    const string sql = "SELECT * FROM students WHERE id = @id;";

    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", id);

    await using var reader = await command.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
        return Results.NotFound(new { message = "Student not found." });

    return Results.Ok(ReadStudent(reader));
});

// Update student
app.MapPut("/api/students/{id:int}", async (int id, StudentRequest student) =>
{
    if (string.IsNullOrWhiteSpace(student.Name) || student.Name.Length > 100)
        return Results.BadRequest("Student name is required and must not exceed 100 characters.");

    if (student.Dob == default)
        return Results.BadRequest("Date of birth is required.");

    const string sql = """
        UPDATE students
        SET name = @name,
            dob = @dob,
            gender = @gender,
            email = @email,
            phone = @phone,
            address = @address,
            grade = @grade
        WHERE id = @id;
        """;

    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);

    command.Parameters.AddWithValue("@id", id);
    command.Parameters.AddWithValue("@name", student.Name.Trim());
    command.Parameters.AddWithValue("@dob", student.Dob.ToDateTime(TimeOnly.MinValue));
    command.Parameters.AddWithValue("@gender", (object?)student.Gender ?? DBNull.Value);
    command.Parameters.AddWithValue("@email", (object?)student.Email ?? DBNull.Value);
    command.Parameters.AddWithValue("@phone", (object?)student.Phone ?? DBNull.Value);
    command.Parameters.AddWithValue("@address", (object?)student.Address ?? DBNull.Value);
    command.Parameters.AddWithValue("@grade", (object?)student.Grade ?? DBNull.Value);

    var affectedRows = await command.ExecuteNonQueryAsync();

    if (affectedRows == 0)
    {
        await using var checkCommand = new MySqlCommand(
            "SELECT COUNT(*) FROM students WHERE id = @id;", connection);
        checkCommand.Parameters.AddWithValue("@id", id);

        var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

        if (!exists)
            return Results.NotFound(new { message = "Student not found." });
    }

    return Results.Ok(new { message = "Student updated successfully." });
});

// Delete student
app.MapDelete("/api/students/{id:int}", async (int id) =>
{
    const string sql = "DELETE FROM students WHERE id = @id;";

    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", id);

    var affectedRows = await command.ExecuteNonQueryAsync();

    if (affectedRows == 0)
        return Results.NotFound(new { message = "Student not found." });

    return Results.Ok(new { message = "Student deleted successfully." });
});

app.Run();


// Convert database row to student response
static StudentResponse ReadStudent(MySqlDataReader reader)
{
    return new StudentResponse(
        reader.GetInt32("id"),
        reader.GetString("name"),
        DateOnly.FromDateTime(reader.GetDateTime("dob")),
        reader.IsDBNull(reader.GetOrdinal("gender")) ? null : reader.GetString("gender"),
        reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email"),
        reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString("phone"),
        reader.IsDBNull(reader.GetOrdinal("address")) ? null : reader.GetString("address"),
        reader.IsDBNull(reader.GetOrdinal("grade")) ? null : reader.GetString("grade")
    );
}

// Database request model
public record StudentRequest(
    string Name,
    DateOnly Dob,
    string? Gender,
    string? Email,
    string? Phone,
    string? Address,
    string? Grade
);

// Database response model
public record StudentResponse(
    int Id,
    string Name,
    DateOnly Dob,
    string? Gender,
    string? Email,
    string? Phone,
    string? Address,
    string? Grade
);
