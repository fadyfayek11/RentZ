<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Rentz</title>
</head>
<body>
    <h1>Rentz</h1>
    <p>Rentz is a web application designed to facilitate the process of renting various types of living spaces, including apartments, villas, duplexes, and more. Built using ASP.NET Core and EF Code First, Rentz offers a user-friendly interface for both administrators and clients (who can also act as owners) to manage and rent properties seamlessly.</p>

    <h2>Features</h2>
    <ul>
        <li><strong>Property Management:</strong> Admins can add, edit, and remove different types of living spaces available for rent, such as apartments, villas, and duplexes.</li>
        <li><strong>User Roles:</strong> Rentz defines two primary user roles:
            <ul>
                <li><em>Admin:</em> Administrators have access to the backend system for managing properties, users, and transactions.</li>
                <li><em>Client (Owner):</em> Clients can list their properties for rent and manage their listings.</li>
            </ul>
        </li>
        <li><strong>SignalR Integration:</strong> Rentz utilizes SignalR for real-time chat functionality and notifications, enabling seamless communication between users and administrators.</li>
    </ul>

    <h2>Technologies Used</h2>
    <ul>
        <li><strong>ASP.NET Core:</strong> A powerful and flexible framework for building web applications and APIs.</li>
        <li><strong>Entity Framework Core (EF):</strong> EF Code First approach is used for database management and migrations.</li>
        <li><strong>SignalR:</strong> A library for adding real-time web functionality to applications, facilitating chat features and instant notifications.</li>
    </ul>
</body>
</html>
