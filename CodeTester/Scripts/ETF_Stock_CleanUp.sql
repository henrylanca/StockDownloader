delete from stockquote
where symbol in (select symbol from etflist)

select * from StockSymbol
where Symbol in (select symbol from etflist)

update stocksymbol
set StartDate=null,
enddate = null
where Symbol in (select symbol from etflist)
