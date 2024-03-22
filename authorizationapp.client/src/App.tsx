import './App.css';

import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Register from './Pages/Register.tsx';
import Navbar from './Components/Navbar.tsx';
import Logout from './Components/Logout.tsx';
import WeatherForecastPage from './Pages/WheatherForecast.tsx';
import Home from './Pages/Home.tsx';
import Login from './Pages/Login.tsx';
import ProfileComponent from './Components/ProfileComponent.tsx';


function App() {
    return (
        <BrowserRouter>
            <Navbar />
            <Routes>
                <Route path="/Account/Login" element={<Login />} />
                <Route path="/Account/Register" element={<Register />} />
                <Route path="/users/logout" element={<Logout />} />
                <Route path="/home" element={<Home />} />
                <Route path="/Account/Manage" element={<ProfileComponent />}></Route>
                <Route path="/" element={<WeatherForecastPage />} />
            </Routes>
        </BrowserRouter>
    )
}
export default App;