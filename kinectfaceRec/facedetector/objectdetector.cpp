#include "objectdetector.h"

const char *facerecAlgorithm = "FaceRecognizer.Fisherfaces";

void initDetector()
{
	try{
		faceDetector.load("lbpcascade_frontalface.xml");
	}
	catch (cv::Exception){
		std::cerr << "ÔØÈëÁ³¼ì²âÆ÷³ö´í" << std::endl;
		exit(1);
	}

	try{
		eyeDetector1.load("haarcascade_eye.xml");
	}
	catch (cv::Exception){
		std::cerr << "ÔØÈëÑÛ¼ì²âÆ÷³ö´í" << endl;
		exit(1);
	}

	if (eyeDetector1.empty())
	{
		std::cerr << "ÔØÈëÑÛ¼ì²âÆ÷³ö´í" << endl;
		exit(1);
	}

	try{
		eyeDetector2.load("haarcascade_eye_tree_eyeglasses.xml");
	}
	catch (cv::Exception){
		std::cerr << "ÔØÈëÑÛ¼ì²âÆ÷³ö´í" << endl;
		exit(1);
	}

	if (eyeDetector2.empty())
	{
		std::cerr << "ÔØÈëÑÛ¼ì²âÆ÷³ö´í" << endl;
		exit(1);
	}

	m_mode = MODE_DETECTION;
	old_time = 0;
}
void facedetector(unsigned char* dimg,  int ScaleWidth,int width,int height)
{
	initDetector();
	sourceimg = cv::Mat(height, width, CV_8UC3, dimg);
	
	
	//detectface
	cv::Mat gray;
	cv::Mat smallImg;
	cv::Mat equalizeImg;
	if (sourceimg.channels() == 3)
	{
		cv::cvtColor(sourceimg, gray, CV_BGR2GRAY);
	}
	else if (sourceimg.channels() == 4)
	{
		cv::cvtColor(sourceimg, gray, CV_BGRA2GRAY);
	}
	else
	{
		gray = sourceimg;
	}

	//imshow("gray", gray);


	float scale = gray.cols / (float)ScaleWidth;
	if (gray.cols > ScaleWidth)
	{
		int detetionHeight = cvRound(gray.rows / scale);
		cv::resize(gray, smallImg, cv::Size(ScaleWidth, detetionHeight));
	}

	cv::equalizeHist(smallImg, equalizeImg);

	//imshow("smallimg", smallImg);
	//imshow("equ", equalizeImg);
	//cvWaitKey(10);
	int flags = cv::CASCADE_SCALE_IMAGE;
	if (m_mode == MODE_DETECTION)
	{
		flags = cv::CASCADE_SCALE_IMAGE;
	}
	else if (m_mode == MODE_COLLECT_FACES)
	{
		flags = cv::CASCADE_FIND_BIGGEST_OBJECT;
	}
	else if(m_mode==MODE_RECOGNITION)
	{
		flags = cv::CASCADE_FIND_BIGGEST_OBJECT;

	}
	cv::Size minFeatureSize(20, 20);
	float searchScaleFactor = 1.1f;
	int minNeighbors = 4;

	
	faceDetector.detectMultiScale(equalizeImg, faces, searchScaleFactor, minNeighbors, flags, minFeatureSize);
	

	if (sourceimg.cols > ScaleWidth)
	{
		for (int i = 0; i < faces.size(); i++)
		{
			faces[i].x = cvRound(faces[i].x* scale);
			faces[i].y = cvRound(faces[i].y * scale);
			faces[i].width = cvRound(faces[i].width * scale);
			faces[i].height = cvRound(faces[i].height * scale);
		}
	}

	for (int i = 0; i < faces.size(); ++i)
	{
		faces[i].x = faces[i].x >= 0 ? faces[i].x : 0;
		faces[i].y = faces[i].y >= 0 ? faces[i].y : 0;
		if (faces[i].width>(sourceimg.cols - faces[i].x))
		{
			faces[i].width = sourceimg.cols - faces[i].x - 1;

		}
		if (faces[i].height > (sourceimg.rows - faces[i].y))
		{
			faces[i].height = sourceimg.rows - faces[i].y - 1;

		}
	}
	if (!faces.empty())
	{
		for (int i = 0; i < faces.size(); ++i)
		{
			rectangle(sourceimg, faces[i], CV_RGB(0, 255, 0), 2, 8, 0);

			cv::Rect *lefteye, *righteye;
			lefteye = &cv::Rect(0, 0, 0, 0);
			righteye = &cv::Rect(0, 0, 0, 0);

			
			Mat faceimg = gray(faces[i]);
			eyedetector(faceimg, eyeDetector1, eyeDetector2, lefteye, righteye, lefteyecenter, righteyecenter);
			if (lefteye)
			{
				lefteye->x += faces[i].x;
				lefteye->y += faces[i].y;
				lefteyecenter.x += faces[i].x;
				lefteyecenter.y += faces[i].y;
				cv::rectangle(sourceimg, *lefteye, CV_RGB(255, 0, 0), 2, 8, 0);
				drawcross(sourceimg, CV_RGB(0, 0, 255), lefteyecenter, 2);


			}
			if (righteye)
			{
				righteye->x += faces[i].x;
				righteye->y += faces[i].y;
				righteyecenter.x += faces[i].x;
				righteyecenter.y += faces[i].y;
				cv::rectangle(sourceimg, *righteye, CV_RGB(255, 0, 0), 2, 8, 0);
				drawcross(sourceimg, CV_RGB(0, 0, 255), righteyecenter, 2);
			}
		}
	}


	Mat preProcessedFace;
	gotFaceAndEyes = false;
	cv::Rect faceRect;
	if(m_mode==MODE_DETECTION)
	{

	}
	else if(m_mode==MODE_COLLECT_FACES)
	{
		faceRect = faces.at(0);
		Mat faceimg = sourceimg(faceRect);
		preProcessedFace = prePreocess(faceimg, lefteyecenter, righteyecenter);
		if(preProcessedFace.data)
		{
			gotFaceAndEyes = true;
		}

		if(gotFaceAndEyes)
		{
			CollectFaces(preProcessedFace, faceRect);
		}

	}
	else if(m_mode==MODE_TRAINING)
	{
		bool haveEnoughData = true;
		if(strcmp(facerecAlgorithm,"FaceRecognizer.Fisherfaces")==0)
		{

		}
	}


	//cv::Mat mask(img.size(), CV_8UC1,cv::Scalar(255));
	//img.setTo(cv::Scalar(0, 0, 0), mask);

	//for (int i = 0; i < width*height * 3; i++)
	//{
		//dimg[i] = 0;
	//}
}

void eyedetector(Mat &img, cv::CascadeClassifier &eyedetect1, cv::CascadeClassifier &eyedetect2, cv::Rect *leftRect, cv::Rect *rightRect, cv::Point &lefteye, cv::Point &righteye)
{
	const float EYE_SX = 0.16f;
	const float EYE_SY = 0.26f;
	const float EYE_SW = 0.30f;
	const float EYE_SH = 0.28f;

	int leftX = cvRound(img.cols*EYE_SX);
	int leftY = cvRound(img.rows*EYE_SY);
	int widthX = cvRound(img.cols*EYE_SW);
	int heightY = cvRound(img.rows*EYE_SH);
	int rightX = cvRound(img.cols - leftX - widthX);
	int rightY = leftY;

	Mat lefteyewindow = img(cv::Rect(leftX, leftY, widthX, heightY));
	Mat righteyewindow = img(cv::Rect(rightX, rightY, widthX, heightY));
	vector<cv::Rect> lefteyeR, righteyeR;

	Mat leftgray, rightgray;
	Mat equalizeimgleft, equalizeimgright;

	if (lefteyewindow.channels() == 3)
	{
		cv::cvtColor(lefteyewindow, leftgray, CV_BGR2GRAY);
		cv::cvtColor(righteyewindow, rightgray, CV_BGR2GRAY);
	}
	else if (lefteyewindow.channels() == 4)
	{
		cv::cvtColor(lefteyewindow, leftgray, CV_BGRA2GRAY);
		cv::cvtColor(righteyewindow, rightgray, CV_BGRA2GRAY);

	}
	else
	{
		leftgray = lefteyewindow;
		rightgray = righteyewindow;
	}

	cv::equalizeHist(leftgray, equalizeimgleft);
	cv::equalizeHist(rightgray, equalizeimgright);

	int flags = cv::CASCADE_FIND_BIGGEST_OBJECT;
	cv::Size minFeatureSize(20, 20);
	float searchScaleFactor = 1.1f;
	int minNeighbors = 4;

	eyedetect1.detectMultiScale(equalizeimgleft, lefteyeR, searchScaleFactor, minNeighbors, flags, minFeatureSize);
	eyedetect1.detectMultiScale(equalizeimgright, righteyeR, searchScaleFactor, minNeighbors, flags, minFeatureSize);

	if (lefteyeR.empty() && !eyedetect2.empty())
	{
		eyedetect2.detectMultiScale(equalizeimgleft, lefteyeR, searchScaleFactor, minNeighbors, flags, minFeatureSize);
	}

	if (righteyeR.empty() && !eyedetect2.empty())
	{
		eyedetect2.detectMultiScale(equalizeimgright, righteyeR, searchScaleFactor, minNeighbors, flags, minFeatureSize);

	}

	if (leftRect)
	{
		if (!lefteyeR.empty())
		{
			*leftRect = lefteyeR.at(0);
			leftRect->x = cvRound(leftRect->x + leftX);
			leftRect->y = cvRound(leftRect->y + leftY);
			lefteye = cv::Point(leftRect->x + leftRect->width / 2, leftRect->y + leftRect->height / 2);
			//drawcross(img, CV_RGB(0, 0, 255), lefteye, 2);
		}
		else
		{
			leftRect = NULL;
			lefteye = cv::Point(-1, -1);
		}
	}
	if (rightRect)
	{
		if (!righteyeR.empty())
		{
			*rightRect = righteyeR.at(0);
			rightRect->x = cvRound(rightRect->x + rightX);
			rightRect->y = cvRound(rightRect->y + rightY);
			righteye = cv::Point(rightRect->x + rightRect->width / 2, rightRect->y + rightRect->height / 2);
			//drawcross(img, CV_RGB(0, 0, 255), righteye, 2);

		}
		else
		{
			rightRect = NULL;
			righteye = cv::Point(-1, -1);
		}
	}

}


void drawcross(Mat &img, const cv::Scalar &color, cv::Point &center, int linewidth)
{
	cv::Point up, down, right, left;
	up = down = right = left = center;
	up.y -= 10;
	down.y += 10;
	right.x += 10;
	left.x -= 10;

	cv::line(img, up, down, color, linewidth);
	cv::line(img, left, right, color, linewidth);
}

Mat prePreocess(Mat &faceimg, cv::Point &leftEye, cv::Point &rightEye)
{
	if(leftEye.x>0&&rightEye.x>0)
	{
		cv::Point2f eyesCenter = cv::Point2f((leftEye.x + rightEye.x) * 0.5f, (leftEye.y + rightEye.y) * 0.5f);
		double dy = (rightEye.y - leftEye.y);
		double dx = (rightEye.x - leftEye.x);
		double len = sqrt(dx*dx + dy*dy);
		double angle = atan2(dy, dx) * 180.0 / CV_PI; // Convert from radians to degrees.

		const double DESIRED_LEFT_EYE_X = 0.16;
		const double DESIRED_LEFT_EYE_Y = 0.14;
		const double DESIRED_RIGHT_EYE_X = (1.0f - DESIRED_LEFT_EYE_X);
		const double DESIRED_RIGHT_EYE_Y = (1.0f - DESIRED_LEFT_EYE_Y);

		const int desiredFaceWidth = 70;
		const int desiredFaceHeight = 70;

		double desiredLen = (DESIRED_RIGHT_EYE_X - DESIRED_LEFT_EYE_X) * desiredFaceWidth;
		double scale = desiredLen / len;
		Mat rot_mat = cv::getRotationMatrix2D(eyesCenter, angle, scale);
		rot_mat.at<double>(0, 2) += desiredFaceWidth * 0.5f - eyesCenter.x;
		rot_mat.at<double>(1, 2) += desiredFaceHeight * DESIRED_LEFT_EYE_Y - eyesCenter.y;

		Mat warped = Mat(desiredFaceHeight, desiredFaceWidth, CV_8U, cv::Scalar(128)); // Clear the output image to a default grey.
		warpAffine(faceimg, warped, rot_mat, warped.size());

		//¾ùºâ»¯ÓëÂË²¨
		equalizeLeftandRight(warped);
		Mat filteredimg = Mat(warped.size(), CV_8U);
		cv::bilateralFilter(warped, filteredimg, 0, 20.0, 2);

		//ÍÖÔ²ÑÚÂë
		Mat mask = Mat(warped.size(), CV_8UC1, cv::Scalar(255));
		cv::Point facecenter = cv::Point(cvRound(desiredFaceWidth*0.5), cvRound(desiredFaceHeight*0.4));
		cv::Size size = cv::Size(cvRound(desiredFaceWidth*0.5), cvRound(desiredFaceHeight*0.8));
		cv::ellipse(mask, facecenter, size, 0, 0, 360, cv::Scalar(0), CV_FILLED);

		filteredimg.setTo(cv::Scalar(128), mask);
		
		return filteredimg;
	}
	else
	{
		Mat tmpImg;
		cv::resize(faceimg, tmpImg, cv::Size(70, 70));
		return tmpImg;
	}
}


void equalizeLeftandRight(Mat &faceimg)
{
	int w = faceimg.cols;
	int h = faceimg.rows;
	Mat wholeFace;
	cv::equalizeHist(faceimg, wholeFace);
	int midx = w / 2;
	Mat leftside = faceimg(cv::Rect(0, 0, midx, h));
	Mat rightside = faceimg(cv::Rect(midx, 0, w - midx, h));

	cv::equalizeHist(leftside, leftside);
	cv::equalizeHist(rightside, rightside);


	for (int y = 0; y < h;y++)
	{
		uchar *facedata = faceimg.ptr<uchar>(y);
		uchar *wholefacedata = wholeFace.ptr<uchar>(y);
		uchar *leftsidedata = leftside.ptr<uchar>(y);
		uchar *rightsidedata = rightside.ptr<uchar>(y);
		for (int x = 0; x < w;x++)
		{
			int value;
			if(x<(w>>2))
			{
				value = *(leftsidedata + x);
			}
			else if(x<(w>>1))
			{
				int lv = *(leftsidedata + x);
				int wv = *(wholefacedata + x);

				float weight = (x - (w >> 2)) / (float)(w >> 2);
				value = cvRound(wv*weight + lv*(1 - weight));
				
			}
			else if(x<(w>>2)*3)
			{
				int rv = *(rightsidedata + x);
				int wv = *(leftsidedata + x);

				float weight = (x - (w >> 2)) / (float)(w >> 2);
				value = cvRound(rv*weight + wv*(1 - weight));

			}
			else
			{
				value = *(rightsidedata + x);
			}

			*(facedata + x) = value;
		}//end x loop
	}//end y loop
}

void CollectFaces(Mat &preprocessed,cv::Rect &face)
{
	
	if (!preprocessed.empty())
	{
		double current_time = (double)cv::getTickCount();		
		double timeDiff_s = (current_time - old_time) / cv::getTickFrequency();
		double imageDiff = 1e30;
		if(old_prefacedata.data)
		{
			imageDiff = getSimilarity(preprocessed, old_prefacedata);

		}
		if(imageDiff>CHANGE_IN_IMAGE_FOR_COLLECTION&&(timeDiff_s>CHANGE_IN_TIME_FOR_COLLECTION))
		{
			Mat mirriredFace;
			cv::flip(preprocessed, mirriredFace, 1);

			m_latestFaces[m_selectedPerson] = preprocessFaces.size() - 2;

			preprocessFaces.push_back(preprocessed);
			preprocessFaces.push_back(mirriredFace);

			faceLabels.push_back(m_selectedPerson);
			faceLabels.push_back(m_selectedPerson);
			old_prefacedata = preprocessed;
			old_time = current_time;

			Mat displayRegion = sourceimg(face);
			displayRegion += CV_RGB(90, 90, 90);
		}
	}
}

double getSimilarity(const Mat A, const Mat B)
{
	double errorL2 = cv::norm(A, B, CV_L2);
	double similarity = errorL2 / (double)(A.rows*A.cols*A.channels());
	return similarity;
}

void Trainfaces(cv::Ptr<cv::FaceRecognizer> &facerecognizer, const vector<Mat> preprocessedFaces, const vector<int> faceLabels, const string facerecAlgorithm = "FaceRecognizer.Eigenfaces")
{
	bool haveContribModule = cv::initModule_contrib();
	if(!haveContribModule)
	{
		std::cerr << "ERROR: The 'contrib' module is needed for FaceRecognizer but has not been loaded into OpenCV!" << endl;
		exit(1);
	}

	facerecognizer = cv::Algorithm::create<cv::FaceRecognizer>(facerecAlgorithm);
	if(facerecognizer.empty())
	{
		std::cerr << "ERROR: The FaceRecognizer algorithm [" << facerecAlgorithm << "] is not available in your version of OpenCV. Please update to OpenCV v2.4.1 or newer." << endl;
		exit(1);
	}

	facerecognizer->train(preprocessedFaces, faceLabels);
	facerecognizer->save("trainmodel.yml");
}

void RecognizeFace(cv::Ptr<cv::FaceRecognizer> &facerecognizer, const Mat preprocessface)
{
	
	
}

Mat reconstructFace(const cv::Ptr<cv::FaceRecognizer> model, const Mat preprocessedFace)
{
	// Since we can only reconstruct the face for some types of FaceRecognizer models (ie: Eigenfaces or Fisherfaces),
	// we should surround the OpenCV calls by a try/catch block so we don't crash for other models.
	try {

		// Get some required data from the FaceRecognizer model.
		Mat eigenvectors = model->get<Mat>("eigenvectors");
		Mat averageFaceRow = model->get<Mat>("mean");

		int faceHeight = preprocessedFace.rows;

		// Project the input image onto the PCA subspace.
		Mat projection = subspaceProject(eigenvectors, averageFaceRow, preprocessedFace.reshape(1, 1));
		//printMatInfo(projection, "projection");

		// Generate the reconstructed face back from the PCA subspace.
		Mat reconstructionRow = subspaceReconstruct(eigenvectors, averageFaceRow, projection);
		//printMatInfo(reconstructionRow, "reconstructionRow");

		// Convert the float row matrix to a regular 8-bit image. Note that we
		// shouldn't use "getImageFrom1DFloatMat()" because we don't want to normalize
		// the data since it is already at the perfect scale.

		// Make it a rectangular shaped image instead of a single row.
		Mat reconstructionMat = reconstructionRow.reshape(1, faceHeight);
		// Convert the floating-point pixels to regular 8-bit uchar pixels.
		Mat reconstructedFace = Mat(reconstructionMat.size(), CV_8U);
		reconstructionMat.convertTo(reconstructedFace, CV_8U, 1, 0);
		//printMatInfo(reconstructedFace, "reconstructedFace");

		return reconstructedFace;

	}
	catch (cv::Exception e) {
		//cout << "WARNING: Missing FaceRecognizer properties." << endl;
		return Mat();
	}
}

Mat getImageFrom1DFloatMat(const Mat matrixRow, int height)
{
	// Make it a rectangular shaped image instead of a single row.
	Mat rectangularMat = matrixRow.reshape(1, height);
	// Scale the values to be between 0 to 255 and store them as a regular 8-bit uchar image.
	Mat dst;
	normalize(rectangularMat, dst, 0, 255, cv::NORM_MINMAX, CV_8UC1);
	return dst;
}