# Use the .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app


# Copy the entire project and build
COPY ProxyServer/ ./ProxyServer/
WORKDIR /app/ProxyServer
RUN dotnet publish -c Release -o /out

# Use a smaller runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Expose port 8080 for the proxy
EXPOSE 8080

# Set environment variables for authentication
ENV PROXY_USERNAME=admin
ENV PROXY_PASSWORD=admin

# Run the proxy server
CMD ["dotnet", "ProxyServer.dll"]
