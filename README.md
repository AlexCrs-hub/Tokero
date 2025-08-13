# Tokero

Credentials:<br>
Username: admin<br> 
Password: password <br>
Other credentials result in invalid login<br>

In order to use the api (CoinGecko, because CoinMarketCap does not have historical data in the free tier) do the following:<br>

Windows<br>
In powershell: setx COINGEKO_API_KEY "your_key"<br>

MacOS<br>
export COINGECKO_API_KEY="your-real-key"<br>

It should use the db clone provided, but if that does not work for some reason, use the API key in the email, or create another on CoinGeko. <br>

Installed coins are the top 10 coins given by the API. <br>