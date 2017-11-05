# Questrade IPO

The program gets the list of all IPO's on the TSX from the IPO Centre and then loops through the list to find those of type "Equity" that are closed. It then fetches the IPO data of each IPO and keeps only those that have a stock symbol in them. Then it downloads the historical stock data from the date of the IPO until now and does various calculations to determine whether it would have been profitable to invest in the IPO's.

It uses the Alpha Vantage API to get historical data. Alpha Vantage doesn't support specifying date ranges so the values are based on the last 100 days of data.

## Results

Example output (profit per share):

```
RRX: 0.42
AAV: 1.04
PVG: -1.87
ENB: 4.3
ECI: -0.58
STN: -5.02
H: 0.79
MTL: -0.8
SBB: -0.4
FRU: -2.71
CNL: 0.34
ERF: -1.1
TOY: -10.97
KEY: 4.85
FCR: 0.26
GEI: 0.42
ALA: 0.85
BNP: -0.04
SU: -4.11
DPM: -0.18
TMR: 6.95
CG: -0.54
BSX: 0.12
BXE: 1.17
FCR: 0.26
IPL: -0.33
X: -2.17
CPG: 1.43
NVA: -1.71
```
