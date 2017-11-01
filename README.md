# Questrade IPO

The program gets the list of all IPO's from the IPO Centre and then loops through the list to find those of type "Equity" that are closed. It then fetches the IPO data of each IPO and keeps only those that have a stock symbol in them. Then it downloads the historical stock data from the date of the IPO until now and does various calculations to determine whether it would have been profitable to invest in the IPO's.
