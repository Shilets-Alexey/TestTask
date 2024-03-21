import AuthorizeView from "../Components/AuthorizeView.tsx";
import UsersComponent from "../Components/UsersComponent.tsx";

function Home() {
    return (
        <AuthorizeView>
            <UsersComponent />
        </AuthorizeView>

    );
}

export default Home;