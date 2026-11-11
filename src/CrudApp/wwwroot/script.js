
const API_URL = "/api/students";

const tableBody = document.getElementById("studentTableBody");
const totalStudents = document.getElementById("totalStudents");
const searchInput = document.getElementById("searchInput");
const addStudentBtn = document.getElementById("addStudentBtn");

let students = [];

// Load students from the API
async function loadStudents() {
    try {
        const response = await fetch(API_URL);

        if (!response.ok) {
            throw new Error("Unable to load students.");
        }

        students = await response.json();
        displayStudents(students);
        totalStudents.textContent = students.length;

    } catch (error) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="6" class="empty-message">
                    Unable to connect to the server.
                </td>
            </tr>
        `;
        console.error(error);
    }
}

// Display students in the table
function displayStudents(data) {
    tableBody.innerHTML = "";

    if (data.length === 0) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="6" class="empty-message">
                    No students found.
                </td>
            </tr>
        `;
        return;
    }

    data.forEach(student => {
        const row = document.createElement("tr");

        const values = [
            student.id,
            student.name,
            student.email || "-",
            student.phone || "-",
            student.grade || "-"
        ];

        values.forEach(value => {
            const cell = document.createElement("td");
            cell.textContent = value;
            row.appendChild(cell);
        });

        const actionsCell = document.createElement("td");

        const editButton = document.createElement("button");
        editButton.textContent = "Edit";
        editButton.className = "action-button edit-button";
        editButton.addEventListener("click", () => openStudentForm(student));

        const deleteButton = document.createElement("button");
        deleteButton.textContent = "Delete";
        deleteButton.className = "action-button delete-button";
        deleteButton.addEventListener("click", () => deleteStudent(student.id));

        actionsCell.append(editButton, deleteButton);
        row.appendChild(actionsCell);

        tableBody.appendChild(row);
    });
}

// Search students
searchInput.addEventListener("input", () => {
    const searchTerm = searchInput.value.toLowerCase();

    const filteredStudents = students.filter(student =>
        student.name.toLowerCase().includes(searchTerm) ||
        (student.email || "").toLowerCase().includes(searchTerm) ||
        (student.grade || "").toLowerCase().includes(searchTerm)
    );

    displayStudents(filteredStudents);
});

// Create the student form modal
function createModal() {
    const modal = document.createElement("div");
    modal.id = "studentModal";
    modal.className = "modal";

    modal.innerHTML = `
        <div class="modal-content">
            <div class="modal-header">
                <div>
                    <h2 id="modalTitle">Add Student</h2>
                    <p>Enter the student information below.</p>
                </div>
                <button type="button" class="close-button" id="closeModal">&times;</button>
            </div>

            <form id="studentForm">
                <div class="form-grid">
                    <div class="form-group">
                        <label for="name">Full Name *</label>
                        <input type="text" id="name" maxlength="100" required>
                    </div>

                    <div class="form-group">
                        <label for="dob">Date of Birth *</label>
                        <input type="date" id="dob" required>
                    </div>

                    <div class="form-group">
                        <label for="gender">Gender</label>
                        <select id="gender">
                            <option value="">Select gender</option>
                            <option value="Male">Male</option>
                            <option value="Female">Female</option>
                            <option value="Other">Other</option>
                        </select>
                    </div>

                    <div class="form-group">
                        <label for="email">Email</label>
                        <input type="email" id="email" maxlength="150">
                    </div>

                    <div class="form-group">
                        <label for="phone">Phone</label>
                        <input type="text" id="phone" maxlength="20">
                    </div>

                    <div class="form-group">
                        <label for="grade">Grade</label>
                        <input type="text" id="grade" maxlength="20">
                    </div>

                    <div class="form-group full-width">
                        <label for="address">Address</label>
                        <textarea id="address" maxlength="255" rows="3"></textarea>
                    </div>
                </div>

                <div class="form-actions">
                    <button type="button" class="cancel-button" id="cancelModal">
                        Cancel
                    </button>
                    <button type="submit" class="save-button">
                        Save Student
                    </button>
                </div>
            </form>
        </div>
    `;

    document.body.appendChild(modal);

    document.getElementById("closeModal").addEventListener("click", closeModal);
    document.getElementById("cancelModal").addEventListener("click", closeModal);
    document.getElementById("studentForm").addEventListener("submit", saveStudent);
}

createModal();

// Open the form for adding or editing
function openStudentForm(student = null) {
    const form = document.getElementById("studentForm");
    form.reset();

    document.getElementById("modalTitle").textContent =
        student ? "Edit Student" : "Add Student";

    document.getElementById("studentModal").dataset.studentId =
        student ? student.id : "";

    if (student) {
        document.getElementById("name").value = student.name;
        document.getElementById("dob").value = student.dob;
        document.getElementById("gender").value = student.gender || "";
        document.getElementById("email").value = student.email || "";
        document.getElementById("phone").value = student.phone || "";
        document.getElementById("grade").value = student.grade || "";
        document.getElementById("address").value = student.address || "";
    }

    document.getElementById("studentModal").classList.add("show");
}

// Close the form
function closeModal() {
    document.getElementById("studentModal").classList.remove("show");
}

// Save a new or updated student
async function saveStudent(event) {
    event.preventDefault();

    const modal = document.getElementById("studentModal");
    const studentId = modal.dataset.studentId;

    const student = {
        name: document.getElementById("name").value.trim(),
        dob: document.getElementById("dob").value,
        gender: document.getElementById("gender").value || null,
        email: document.getElementById("email").value.trim() || null,
        phone: document.getElementById("phone").value.trim() || null,
        address: document.getElementById("address").value.trim() || null,
        grade: document.getElementById("grade").value.trim() || null
    };

    try {
        const response = await fetch(
            studentId ? `${API_URL}/${studentId}` : API_URL,
            {
                method: studentId ? "PUT" : "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(student)
            }
        );

        if (!response.ok) {
            throw new Error(await response.text());
        }

        closeModal();
        await loadStudents();
        alert(studentId ? "Student updated successfully." : "Student added successfully.");

    } catch (error) {
        alert(error.message || "Unable to save student.");
    }
}

// Delete a student
async function deleteStudent(id) {
    if (!confirm("Are you sure you want to delete this student?")) {
        return;
    }

    try {
        const response = await fetch(`${API_URL}/${id}`, {
            method: "DELETE"
        });

        if (!response.ok) {
            throw new Error("Unable to delete student.");
        }

        await loadStudents();
        alert("Student deleted successfully.");

    } catch (error) {
        alert(error.message);
    }
}

// Open the add student form
addStudentBtn.addEventListener("click", () => openStudentForm());

// Load students when the page opens
loadStudents();
