<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SignOut.aspx.cs" Inherits="WebLayer.SignOut" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Signing out...</title>
    <style>
        #signout
        {
            display:block;
            height:auto;
            font-family:sans-serif;
            font-size:x-large;
            text-align:center;
            vertical-align:middle;
        }
        #signout img
        {
            position:relative;
            top:-2px;
            vertical-align:middle;
            padding-right:5px;
        }
    </style>
    <script>alert("woop!");</script>
</head>
<body>
    <img src="./images/rclogo.png" height="30" width="255" />
    <div id="signout"><img src="images/progress.gif" />Signing out...</div>
</body>
</html>