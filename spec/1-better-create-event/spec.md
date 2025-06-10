goals:
0. query speedhive,  filter the json for group 1 races, use those as the raceResultUris later in the event-template
    1. query my profile, request url: https://eventresults-api.speedhive.com/api/v0.2.3/eventresults/accounts/MYLAPS-GA-fee46dbc16df4da7ad0a8f8c973a563d/events?sportCategory=Motorized&count=100
    2. select the latest event by Cascade Sports Car Club or IRDC, query that url for the groups: https://eventresults-api.speedhive.com/api/v0.2.3/eventresults/events/3032241?sessions=true
1. automatically soft link in the "latest" prize-descriptions and car-to-sticker-mapping files to the deluxxe sub directory in the event
2. automatically copy the event-template.json file into the deluxxe sub directory, name it deluxxe.json
3. update the config, replacing the bracketed elements