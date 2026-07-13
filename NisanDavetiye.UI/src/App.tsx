import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { InvitationPage } from './pages/InvitationPage'
import { AdminPage } from './pages/AdminPage'
import { HomePage } from './pages/HomePage'
import './index.css'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/i/:inviteId" element={<InvitationPage />} />
        <Route path="/p/:panelUid" element={<AdminPage />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
