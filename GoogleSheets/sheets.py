from __future__ import print_function
import sys
import pickle
import os.path
from googleapiclient.discovery import build
from google_auth_oauthlib.flow import InstalledAppFlow
from google.auth.transport.requests import Request
from googleapiclient.errors import HttpError
import tools

SCOPES = ['https://www.googleapis.com/auth/spreadsheets.readonly']
SPREADSHEET_ID = '1us8XhVosIuf0IU1X2IB41aAqr-77UN0Myw7_80xq0-E'


class sheets:

    def __init__(self):
        self.CREDS = self.auth()
        self.SERVICE = build('sheets', 'v4', credentials=self.CREDS)
        self.SHEET = self.SERVICE.spreadsheets()

    def auth(self):
        self.CREDS = None
        # The file token.pickle stores the user's access and refresh tokens, and is
        # created automatically when the authorization flow completes for the first
        # time.
        if os.path.exists('token.pickle'):
            with open('token.pickle', 'rb') as token:
                self.CREDS = pickle.load(token)
        # If there are no (valid) credentials available, let the user log in.
        if not self.CREDS or not self.CREDS.valid:
            if self.CREDS and self.CREDS.expired and self.CREDS.refresh_token:
                self.CREDS.refresh(Request())
            else:
                flow = InstalledAppFlow.from_client_secrets_file(
                    'credentials.json', SCOPES)
                self.CREDS = flow.run_local_server(port=0)
            # Save the credentials for the next run
            with open('token.pickle', 'wb') as token:
                pickle.dump(self.CREDS, token)
        return self.CREDS

    def get_range(self, myrange):
        try:
            result = self.SHEET.values().get(spreadsheetId=SPREADSHEET_ID, range=myrange).execute()
        except HttpError:
            sys.exit(429)
        values = result.get('values', [])
        return values

    def get_headers_column(self, header):
        columns = len(header)
        name = ""
        if columns > 25:
            columns -= 25
            name += "A"
        name += chr(64 + columns)
        return name

    def GetHeaderAndTypes(self, sheet_name: str):
        values = self.get_range(sheet_name + "!A1:AZ2")
        return values[1], values[0]

    def GetTableDic(self, sheet_name: str, header: [], types: []):
        values = self.get_range(sheet_name + "!A3:" + self.get_headers_column(header) + "10002")
        return {int(row[0]): row_to_obj(row, header, types) for row in values}

    def GetTableList(self, sheet_name: str, header: [], types: []):
        values = self.get_range(sheet_name + "!A3:" + self.get_headers_column(header) + "10002")
        return [row_to_obj(row, header, types) for row in values]


def row_to_obj(row, header, types):
    return {header[i]: tools.getNormalType(types[i], (row[i] if i < len(row) else "")) for i in range(len(header))}
