import { useState, useEffect } from 'react'
import './App.css'

interface Task {
  id: string
  title: string
  description?: string
  status: 'ToDo' | 'InProgress' | 'Done'
  dueAt?: string
}

function App() {
  const [tasks, setTasks] = useState<Task[]>([])
  const [health, setHealth] = useState<string>('Checking...')
  const [newTask, setNewTask] = useState({ title: '', description: '' })

  const API_BASE = 'https://localhost:7124'

  // Check health
  useEffect(() => {
    fetch(`${API_BASE}/health`)
      .then(res => res.json())
      .then(data => setHealth(data.status))
      .catch(() => setHealth('Offline'))
  }, [])

  // Load tasks
  useEffect(() => {
    fetch(`${API_BASE}/tasks`)
      .then(res => res.json())
      .then(setTasks)
      .catch(console.error)
  }, [])

  const addTask = async () => {
    if (!newTask.title) return

    try {
      const res = await fetch(`${API_BASE}/tasks`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newTask)
      })
      const task = await res.json()
      setTasks([...tasks, task])
      setNewTask({ title: '', description: '' })
    } catch (error) {
      console.error('Failed to add task:', error)
    }
  }

  const updateStatus = async (id: string, status: string) => {
    try {
      await fetch(`${API_BASE}/tasks/${id}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status })
      })
      setTasks(tasks.map(t => t.id === id ? { ...t, status: status as any } : t))
    } catch (error) {
      console.error('Failed to update task:', error)
    }
  }

  const deleteTask = async (id: string) => {
    try {
      await fetch(`${API_BASE}/tasks/${id}`, { method: 'DELETE' })
      setTasks(tasks.filter(t => t.id !== id))
    } catch (error) {
      console.error('Failed to delete task:', error)
    }
  }

  return (
    <div className="app">
      <header>
        <h1>Caseworker Tasks</h1>
        <div className={`health ${health.toLowerCase()}`}>
          API: {health}
        </div>
      </header>

      <div className="add-task">
        <input
          placeholder="Task title"
          value={newTask.title}
          onChange={e => setNewTask({ ...newTask, title: e.target.value })}
        />
        <input
          placeholder="Description (optional)"
          value={newTask.description}
          onChange={e => setNewTask({ ...newTask, description: e.target.value })}
        />
        <button onClick={addTask}>Add Task</button>
      </div>

      <div className="tasks">
        {tasks.map(task => (
          <div key={task.id} className={`task ${task.status.toLowerCase()}`}>
            <div className="task-info">
              <h3>{task.title}</h3>
              {task.description && <p>{task.description}</p>}
              {task.dueAt && <small>Due: {new Date(task.dueAt).toLocaleDateString()}</small>}
            </div>
            <div className="task-actions">
              <select
                value={task.status}
                onChange={e => updateStatus(task.id, e.target.value)}
              >
                <option value="ToDo">To Do</option>
                <option value="InProgress">In Progress</option>
                <option value="Done">Done</option>
              </select>
              <button onClick={() => deleteTask(task.id)}>Delete</button>
            </div>
          </div>
        ))}
      </div>

      {tasks.length === 0 && (
        <div className="empty">
          No tasks yet. Add one above!
        </div>
      )}
    </div>
  )
}

export default App