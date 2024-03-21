import AuthorizeView from "../Components/AuthorizeView";
import WeatherForecastComponent from "../Components/WeatherForecastComponent";

function WeatherForecast() {
    return (
        <AuthorizeView>
            <WeatherForecastComponent />
        </AuthorizeView>
        
    );
}

export default WeatherForecast;