//
//  NativeUtils.h
//  UnityFramework
//
//  Created by fotoable on 2020/12/7.
//

#ifndef NativeUtils_h
#define NativeUtils_h

extern "C"{
    extern void saveUDID(const char *str);
    extern char * getUDID();
    extern char* MakeStringCopy1 (const char* string);
    extern char * GetCurrLanguage();
    extern char * GetCurrCountry();
    extern char * GetCurrCurrencyCode();
    extern char *getIDFA();
    extern void performVibrate();
    extern void RequestATTAuthorization();
    extern bool CheckIsUserDarkMode();
}

#endif /* NativeUtils_h */
