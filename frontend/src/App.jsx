import { useState, useEffect } from 'react'
import './App.css'

function App() {
  const [vets, setVets] = useState([])
  const [newVet, setNewVet] = useState({ fullName: '', email: '' })
  const [loading, setLoading] = useState(false)

  // API base URL - points to your backend
  const API_BASE = import.meta.env.VITE_API_BASE + '/api/vets';

  // Fetch all vets from backend
  const fetchVets = async () => {
    try {
      setLoading(true)
      const response = await fetch(API_BASE)
      if (!response.ok) throw new Error('Failed to fetch vets')
      const data = await response.json()
      setVets(data)
    } catch (error) {
      console.error('Error fetching vets:', error)
      alert('Error loading vets. Is the backend running?')
    } finally {
      setLoading(false)
    }
  }

  // Add a new vet
  const addVet = async () => {
    if (!newVet.fullName || !newVet.email) {
      alert('Please fill in all fields')
      return
    }

    try {
      const response = await fetch(API_BASE, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(newVet)
      })

      if (!response.ok) throw new Error('Failed to create vet')
      
      // Clear form and refresh list
      setNewVet({ fullName: '', email: '' })
      fetchVets()
      alert('Vet added successfully!')
    } catch (error) {
      console.error('Error adding vet:', error)
      alert('Error adding vet. Maybe the email already exists?')
    }
  }

  // Delete a vet
  const deleteVet = async (id) => {
    if (!window.confirm('Are you sure you want to delete this vet?')) return

    try {
      const response = await fetch(`${API_BASE}/${id}`, {
        method: 'DELETE'
      })

      if (!response.ok) throw new Error('Failed to delete vet')
      
      fetchVets() // Refresh the list
      alert('Vet deleted successfully!')
    } catch (error) {
      console.error('Error deleting vet:', error)
      alert('Error deleting vet')
    }
  }

  // Load vets when component mounts
  useEffect(() => {
    fetchVets()
  }, [])

  return (
    <div className="app">
      <h1>Vet Management System</h1>
      
      {/* Add New Vet Form */}
      <div className="add-form">
        <h2>Add New Vet</h2>
        <input
          type="text"
          placeholder="Full Name"
          value={newVet.fullName}
          onChange={(e) => setNewVet({...newVet, fullName: e.target.value})}
        />
        <input
          type="email"
          placeholder="Email"
          value={newVet.email}
          onChange={(e) => setNewVet({...newVet, email: e.target.value})}
        />
        <button onClick={addVet}>Add Vet</button>
      </div>

      {/* Vets List */}
      <div className="vets-list">
        <h2>Veterinarians ({vets.length})</h2>
        {loading && <p>Loading...</p>}
        
        {vets.length === 0 && !loading ? (
          <p>No vets found. Add one above!</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Email</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {vets.map(vet => (
                <tr key={vet.id}>
                  <td>{vet.id}</td>
                  <td>{vet.fullName}</td>
                  <td>{vet.email}</td>
                  <td>
                    <button 
                      onClick={() => deleteVet(vet.id)}
                      className="delete-btn"
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  )
}

export default App