@ECHO OFF

echo Setting config values
set LOG_LEVEL=info
set AWS_KEY_ID=
set AWS_KEY=
set AWS_REGION=us-west-2
set S3_BUCKET=
set EMAIL_HOST=smtp.office365.com
set EMAIL_PORT=587
set EMAIL_SENDER_ADDRESS=
set EMAIL_SENDER_PWD=
set EMAIL_RECIPIENTS=
echo Done setting config values

echo Installing npm modules
call npm install

echo Running scraper
call npm start

ECHO ON
