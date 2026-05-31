const API_BASE = '/api';

const api = {
    // Departments
    getDepartments: async () => {
        try {
            const response = await fetch(`${API_BASE}/departments`);
            if (!response.ok) throw new Error('Failed to fetch departments');
            return await response.json();
        } catch (error) {
            console.error('Error fetching departments:', error);
            throw error;
        }
    },

    getDepartment: async (id) => {
        try {
            const response = await fetch(`${API_BASE}/departments/${id}`);
            if (!response.ok) throw new Error('Failed to fetch department');
            return await response.json();
        } catch (error) {
            console.error('Error fetching department:', error);
            throw error;
        }
    },

    createDepartment: async (name) => {
        try {
            const response = await fetch(`${API_BASE}/departments`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name })
            });
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to create department');
            }
            return await response.json();
        } catch (error) {
            console.error('Error creating department:', error);
            throw error;
        }
    },

    updateDepartment: async (id, name) => {
        try {
            const response = await fetch(`${API_BASE}/departments/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id, name })
            });
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to update department');
            }
            return await response.json();
        } catch (error) {
            console.error('Error updating department:', error);
            throw error;
        }
    },

    deleteDepartment: async (id) => {
        try {
            const response = await fetch(`${API_BASE}/departments/${id}`, {
                method: 'DELETE'
            });
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to delete department');
            }
            return await response.json();
        } catch (error) {
            console.error('Error deleting department:', error);
            throw error;
        }
    },

    // Professors
    getProfessors: async () => {
        try {
            const response = await fetch(`${API_BASE}/professors`);
            if (!response.ok) throw new Error('Failed to fetch professors');
            return await response.json();
        } catch (error) {
            console.error('Error fetching professors:', error);
            throw error;
        }
    },

    getProfessor: async (id) => {
        try {
            const response = await fetch(`${API_BASE}/professors/${id}`);
            if (!response.ok) throw new Error('Failed to fetch professor');
            return await response.json();
        } catch (error) {
            console.error('Error fetching professor:', error);
            throw error;
        }
    },

    createProfessor: async (professor) => {
        try {
            const response = await fetch(`${API_BASE}/professors`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(professor)
            });
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to create professor');
            }
            return await response.json();
        } catch (error) {
            console.error('Error creating professor:', error);
            throw error;
        }
    },

    updateProfessor: async (id, professor) => {
        try {
            const response = await fetch(`${API_BASE}/professors/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ ...professor, id })
            });
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to update professor');
            }
            return await response.json();
        } catch (error) {
            console.error('Error updating professor:', error);
            throw error;
        }
    },

    deleteProfessor: async (id) => {
        try {
            const response = await fetch(`${API_BASE}/professors/${id}`, {
                method: 'DELETE'
            });
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to delete professor');
            }
            return await response.json();
        } catch (error) {
            console.error('Error deleting professor:', error);
            throw error;
        }
    },

    // Reports
    getReport: async () => {
        try {
            const response = await fetch(`${API_BASE}/reports`);
            if (!response.ok) throw new Error('Failed to fetch report');
            return await response.json();
        } catch (error) {
            console.error('Error fetching report:', error);
            throw error;
        }
    }
};
