goals:
0. download the latest version of the google sheet at a given url
  1. https://docs.google.com/spreadsheets/d/1JS7FgkgCiFziqJdoSolQWCRuBMBuv-Pe2BWkMgdVg9M/edit?gid=1723871199#gid=1723871199
1. parse the csv, selecting the same columns as show in the current car-to-sticker-mapping-*.csv files
2. write the selected columns to the output directory, matching the same naming pattern

constraints
1. do not hard code tokens, or user ids, make those loaded from some file that is not checked in to git