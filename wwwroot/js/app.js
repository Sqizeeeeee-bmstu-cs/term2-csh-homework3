const app = {
    currentSection: 'home',

    // UI Navigation
    showSection: function(sectionId) {
        // Hide all sections
        document.querySelectorAll('.section').forEach(s => s.classList.remove('active'));
        // Show selected section
        document.getElementById(sectionId).classList.add('active');
        // Update active nav link
        document.querySelectorAll('.navbar-nav .nav-link').forEach(link => link.classList.remove('active'));
        event && event.target.classList.add('active');
        this.currentSection = sectionId;
    },

    showDepartments: async function() {
        this.showSection('departments');
        await this.loadDepartments();
    },

    showProfessors: async function() {
        this.showSection('professors');
        await this.loadDepartmentsForSelect();
        await this.loadProfessors();
    },

    showReports: async function() {
        this.showSection('reports');
        await this.loadReports();
    },

    showAlert: function(message, type = 'danger') {
        const alertDiv = document.getElementById('alert');
        alertDiv.className = `alert alert-${type} show`;
        alertDiv.textContent = message;
        setTimeout(() => {
            alertDiv.classList.remove('show');
        }, 5000);
    },

    // Departments
    loadDepartments: async function() {
        try {
            const departments = await api.getDepartments();
            this.renderDepartmentsList(departments);
        } catch (error) {
            this.showAlert('Ошибка при загрузке кафедр: ' + error.message);
        }
    },

    renderDepartmentsList: function(departments) {
        const html = departments.length === 0 
            ? '<div class="alert alert-info">Нет данных</div>'
            : `<table class="table table-striped table-hover">
                <thead class="table-dark">
                    <tr>
                        <th>ID</th>
                        <th>Название</th>
                        <th>Действия</th>
                    </tr>
                </thead>
                <tbody>
                    ${departments.map(d => `
                        <tr>
                            <td>${d.id}</td>
                            <td>${d.name}</td>
                            <td>
                                <button class="btn btn-sm btn-primary" onclick="app.editDepartment(${d.id})">Редакт.</button>
                                <button class="btn btn-sm btn-danger" onclick="app.deleteDepartmentConfirm(${d.id})">Удалить</button>
                            </td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>`;
        document.getElementById('departmentsList').innerHTML = html;
    },

    showDepartmentForm: function() {
        document.getElementById('departmentsForm').style.display = 'block';
        document.getElementById('deptId').value = '';
        document.getElementById('deptName').value = '';
        document.getElementById('departmentFormTitle').textContent = 'Добавить кафедру';
    },

    hideDepartmentForm: function() {
        document.getElementById('departmentsForm').style.display = 'none';
    },

    editDepartment: async function(id) {
        try {
            const dept = await api.getDepartment(id);
            document.getElementById('deptId').value = dept.id;
            document.getElementById('deptName').value = dept.name;
            document.getElementById('departmentFormTitle').textContent = 'Редактировать кафедру';
            document.getElementById('departmentsForm').style.display = 'block';
        } catch (error) {
            this.showAlert('Ошибка при загрузке кафедры: ' + error.message);
        }
    },

    saveDepartment: async function(event) {
        event.preventDefault();
        const id = document.getElementById('deptId').value;
        const name = document.getElementById('deptName').value;

        if (!name.trim()) {
            this.showAlert('Название кафедры не может быть пустым');
            return;
        }

        try {
            if (id) {
                await api.updateDepartment(parseInt(id), name);
                this.showAlert('Кафедра успешно обновлена', 'success');
            } else {
                await api.createDepartment(name);
                this.showAlert('Кафедра успешно создана', 'success');
            }
            this.hideDepartmentForm();
            await this.loadDepartments();
        } catch (error) {
            this.showAlert('Ошибка: ' + error.message);
        }
    },

    deleteDepartmentConfirm: async function(id) {
        try {
            const dept = await api.getDepartment(id);
            if (dept.professors && dept.professors.length > 0) {
                this.showAlert(`Невозможно удалить кафедру, так как с ней связаны ${dept.professors.length} преподаватель(ей)`);
                return;
            }
            if (confirm(`Вы уверены, что хотите удалить кафедру "${dept.name}"?`)) {
                await api.deleteDepartment(id);
                this.showAlert('Кафедра успешно удалена', 'success');
                await this.loadDepartments();
            }
        } catch (error) {
            this.showAlert('Ошибка: ' + error.message);
        }
    },

    // Professors
    loadProfessors: async function() {
        try {
            const professors = await api.getProfessors();
            this.renderProfessorsList(professors);
        } catch (error) {
            this.showAlert('Ошибка при загрузке преподавателей: ' + error.message);
        }
    },

    loadDepartmentsForSelect: async function() {
        try {
            const departments = await api.getDepartments();
            const select = document.getElementById('profDept');
            select.innerHTML = '<option value="">-- Выберите кафедру --</option>';
            departments.forEach(d => {
                const option = document.createElement('option');
                option.value = d.id;
                option.textContent = d.name;
                select.appendChild(option);
            });
        } catch (error) {
            this.showAlert('Ошибка при загрузке кафедр: ' + error.message);
        }
    },

    renderProfessorsList: function(professors) {
        const html = professors.length === 0 
            ? '<div class="alert alert-info">Нет данных</div>'
            : `<table class="table table-striped table-hover">
                <thead class="table-dark">
                    <tr>
                        <th>ID</th>
                        <th>Имя</th>
                        <th>Кафедра</th>
                        <th>Публикации</th>
                        <th>Действия</th>
                    </tr>
                </thead>
                <tbody>
                    ${professors.map(p => `
                        <tr>
                            <td>${p.id}</td>
                            <td>${p.name}</td>
                            <td>${p.departmentName}</td>
                            <td>${p.publications}</td>
                            <td>
                                <button class="btn btn-sm btn-primary" onclick="app.editProfessor(${p.id})">Редакт.</button>
                                <button class="btn btn-sm btn-danger" onclick="app.deleteProfessorConfirm(${p.id})">Удалить</button>
                            </td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>`;
        document.getElementById('professorsList').innerHTML = html;
    },

    showProfessorForm: function() {
        document.getElementById('professorsForm').style.display = 'block';
        document.getElementById('profId').value = '';
        document.getElementById('profForm').reset();
        document.getElementById('profFormTitle').textContent = 'Добавить преподавателя';
    },

    hideProfessorForm: function() {
        document.getElementById('professorsForm').style.display = 'none';
    },

    editProfessor: async function(id) {
        try {
            const prof = await api.getProfessor(id);
            document.getElementById('profId').value = prof.id;
            document.getElementById('profName').value = prof.name;
            document.getElementById('profDept').value = prof.departmentId;
            document.getElementById('profPublications').value = prof.publications;
            document.getElementById('profFormTitle').textContent = 'Редактировать преподавателя';
            document.getElementById('professorsForm').style.display = 'block';
        } catch (error) {
            this.showAlert('Ошибка при загрузке преподавателя: ' + error.message);
        }
    },

    saveProfessor: async function(event) {
        event.preventDefault();
        const id = document.getElementById('profId').value;
        const professor = {
            name: document.getElementById('profName').value,
            departmentId: parseInt(document.getElementById('profDept').value),
            publications: parseInt(document.getElementById('profPublications').value)
        };

        if (!professor.name.trim()) {
            this.showAlert('Имя преподавателя не может быть пустым');
            return;
        }

        if (professor.publications < 0) {
            this.showAlert('Количество публикаций не может быть отрицательным');
            return;
        }

        if (professor.departmentId <= 0) {
            this.showAlert('Выберите кафедру');
            return;
        }

        try {
            if (id) {
                await api.updateProfessor(parseInt(id), professor);
                this.showAlert('Преподаватель успешно обновлён', 'success');
            } else {
                await api.createProfessor(professor);
                this.showAlert('Преподаватель успешно создан', 'success');
            }
            this.hideProfessorForm();
            await this.loadProfessors();
        } catch (error) {
            this.showAlert('Ошибка: ' + error.message);
        }
    },

    deleteProfessorConfirm: async function(id) {
        try {
            const prof = await api.getProfessor(id);
            if (confirm(`Вы уверены, что хотите удалить преподавателя "${prof.name}"?`)) {
                await api.deleteProfessor(id);
                this.showAlert('Преподаватель успешно удалён', 'success');
                await this.loadProfessors();
            }
        } catch (error) {
            this.showAlert('Ошибка: ' + error.message);
        }
    },

    // Reports
    loadReports: async function() {
        try {
            const report = await api.getReport();
            this.renderReports(report);
        } catch (error) {
            this.showAlert('Ошибка при загрузке отчётов: ' + error.message);
        }
    },

    renderReports: function(report) {
        // Section 1
        const section1Html = report.section1.length === 0 
            ? '<div class="alert alert-info">Нет данных</div>'
            : `<table class="table table-striped table-hover">
                <thead class="table-dark">
                    <tr>
                        <th>Имя преподавателя</th>
                        <th>Кафедра</th>
                        <th class="text-center">Публикации</th>
                    </tr>
                </thead>
                <tbody>
                    ${report.section1.map(item => `
                        <tr>
                            <td>${item.name}</td>
                            <td>${item.departmentName}</td>
                            <td class="text-center">${item.publications}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>`;
        document.getElementById('report1').innerHTML = section1Html;

        // Section 2
        const section2Html = report.section2.length === 0 
            ? '<div class="alert alert-info">Нет данных</div>'
            : `<table class="table table-striped table-hover">
                <thead class="table-dark">
                    <tr>
                        <th>Кафедра</th>
                        <th class="text-center">Количество преподавателей</th>
                    </tr>
                </thead>
                <tbody>
                    ${report.section2.map(item => `
                        <tr>
                            <td>${item.department}</td>
                            <td class="text-center">${item.count}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>`;
        document.getElementById('report2').innerHTML = section2Html;

        // Section 3
        const section3Html = report.section3.length === 0 
            ? '<div class="alert alert-info">Нет данных</div>'
            : `<table class="table table-striped table-hover">
                <thead class="table-dark">
                    <tr>
                        <th>Кафедра</th>
                        <th class="text-center">Среднее количество публикаций</th>
                    </tr>
                </thead>
                <tbody>
                    ${report.section3.map(item => `
                        <tr>
                            <td>${item.department}</td>
                            <td class="text-center">${item.avgPublications.toFixed(2)}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>`;
        document.getElementById('report3').innerHTML = section3Html;
    }
};

// Initialize app on page load
document.addEventListener('DOMContentLoaded', () => {
    app.showSection('home');
});
