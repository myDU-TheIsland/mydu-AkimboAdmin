const path = require('path');

module.exports = {
  entry: {
    AkimboAdminHud: './AkimboAdminHud.js',           // Your main file
    AkimboAdminConstructHud: './AkimboAdminConstructHud.js', // Additional file
    AkimboAdminElementHud: './AkimboAdminElementHud.js',
  },
  output: {
    filename: '[name].js',  // [name] will be replaced with the key in the entry object
    path: path.resolve(__dirname, '../dist/ModAkimboAdmin/'),  // Go up one level and place dist in the root folder
  },
  module: {
    rules: [
      {
        test: /\.css$/, // Apply the rule to .css files
        use: ['style-loader', 'css-loader'], // Use these loaders for CSS files
      },
    ],
  },
  mode: 'production',
};
