window.onload = function () {
    // Build a system
    const ui = SwaggerUIBundle({
        url: "/swagger/docs/v1", 
        dom_id: '#swagger-ui',
        deepLinking: true,
        presets: [
            SwaggerUIBundle.presets.apis,
            SwaggerUIStandalonePreset
        ],
        layout: "StandaloneLayout"
    });

    window.ui = ui;
};
