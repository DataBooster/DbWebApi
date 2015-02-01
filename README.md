# DbWebApi

### What is it?

DbWebApi is a .Net library that implement an entirely generic Web API for data driven applications. It acts as a proxy service for web clients to call database (Oracle + SQL Server) stored procedures or functions out-of-box without any configuration or extra coding, the http response JSON or XML will have all Result Sets, Output Parameters and Return Value. If client request a CSV format (accept: text/csv), the http response will transmit the first result set as a CSV stream for almost unlimited number of rows.
