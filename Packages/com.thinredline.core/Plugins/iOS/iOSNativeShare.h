#import "UnityAppController.h"

@interface iOSNativeShare : UIViewController
{
    UINavigationController *navController;
}


struct ConfigStruct {
    char* title;
    char* message;
};

struct SocialSharingStruct {
    char* text;
    char* subject;
	char* filePaths;
};


#ifdef __cplusplus
extern "C" {
#endif
    
    void showAlertMessage(struct ConfigStruct *confStruct);
    void showSocialSharing(struct SocialSharingStruct *confStruct);
    
    void shareTextToWhatsapp(const char *msg);
    bool checkAppInstalled(const char *name);
#ifdef __cplusplus
}
#endif


@end
