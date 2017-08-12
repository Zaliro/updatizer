# Updatizer
Updatizer is a rudimentary way to follow your assets value.

<p align="left">
  <img src="http://i.imgur.com/ZB5pJEW.gif"/>
</p>

## Configuration

You must edit <i>holdings.json</i> before starting Updatizer.<br>

<b>UpdateInterval</b> : Time between wallets estimations <i>(20, 30, 60, [...] in seconds)</i>.<br>
<b>TargetCurrency</b> : Your local currency <i>(USD, EUR, AUD, BGN, BRL, CAD, CHF, CNY, CZK, DKK, GBP, HKD, HRK, HUF, IDR, ILS, INR, JPY, KRW, MXN, MYR, NOK, NZD, PHP, PLN, RON, RUB, SEK, SGD, THB, TRY, ZAR)</i>.<br>
<b>Wallets</b>: Your wallets content <i>(See below)</i>.

<u>This is a sample configuration :</u>

```javascript
{
	"UpdateInterval": 10,
	"TargetCurrency": "USD",
	"Wallets": [
		{
			"Label": "Wallet 1",
			"Holdings": [
				{
					"Name": "BTC",
					"Value": 1
				},
				{
					"Name": "ETH",
					"Value": 20
				},
				{
					"Name": "NEO",
					"Value": 300
				}
			]
		},
		{
			"Label": "Wallet 2",
			"Holdings": [
				{
					"Name": "BTC",
					"Value": 5
				},
				{
					"Name": "PART",
					"Value": 450
				}
			]
		}
	]
}
```