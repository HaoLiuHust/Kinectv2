#define _CRT_SECURE_NO_WARNINGS
#ifndef OBJECTDETECTOR_H
#define OBJECTDETECTOR_H
#include <iostream>
#include <vector>
#include <string>
#include "cv.h"
#include "highgui.h"
#include "cxcore.h"
#include "opencv2/opencv.hpp"
using cv::Mat;
using std::vector;
using std::endl;
using std::string;

extern "C" _declspec(dllexport) void facedetector(unsigned char *img, int ScaleWidth, int width, int height);


void eyedetector(Mat &faceimg, cv::CascadeClassifier &eyedetect1, cv::CascadeClassifier &eyedetect2, cv::Rect *leftRect, cv::Rect *rightRect, cv::Point &lefteye, cv::Point &righteye);
void drawcross(Mat &faceimg, const cv::Scalar &color, cv::Point &center, int linewidth);
Mat preProcess(Mat &faceimg, cv::Point &lefteyecenter, cv::Point &righteyecenter);
void equalizeLeftandRight(Mat &faceimg);
void CollectFaces(Mat &preprocessed,cv::Rect &face);
double getSimilarity(const Mat A, const Mat B);

void Trainfaces(cv::Ptr<cv::FaceRecognizer> &facerecognizer, const vector<Mat> preprocessedFaces, const vector<int> faceLabels, const string facerecAlgorithm = "FaceRecognizer.Eigenfaces");
void RecognizeFace(cv::Ptr<cv::FaceRecognizer> &facerecognizer, const Mat preprocessface);
Mat getImageFrom1DFloatMat(const Mat matrixRow, int height);
Mat reconstructFace(const cv::Ptr<cv::FaceRecognizer> model, const Mat preprocessedFace);

cv::CascadeClassifier faceDetector;
cv::CascadeClassifier eyeDetector1, eyeDetector2;
cv::Mat sourceimg;
std::vector<cv::Rect> faces;

vector<int> m_latestFaces;
cv::Point lefteyecenter, righteyecenter;
vector<Mat> preprocessFaces;
vector<int> faceLabels;
int m_selectedPerson;
double old_time;
Mat old_prefacedata;
bool gotFaceAndEyes;


const double CHANGE_IN_IMAGE_FOR_COLLECTION = 0.3;
const double CHANGE_IN_TIME_FOR_COLLECTION = 1.0;
const double UNKNOWN_PERSON_THRESHOLD = 0.5;

enum MODES
{
	MODE_STARTUP = 0, MODE_DETECTION, MODE_COLLECT_FACES, MODE_TRAINING, MODE_RECOGNITION, MODE_DELETE_ALL, MODE_END
};

MODES m_mode;
void initDetector();
#endif